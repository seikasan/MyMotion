"""Generate the benchmark charts used by README.md and docs/performance.md.

Usage (from the repository root):
    python docs/images/make_charts.py

Reads docs/results/cross-library-current-20261125.csv and writes
SVG charts (white background) next to this script. Values are medians over the five launches per
library and workload, the same numbers as the tables in docs/performance.md.
"""
import csv
import statistics
from collections import defaultdict
from pathlib import Path
from xml.sax.saxutils import escape

ROOT = Path(__file__).resolve().parents[2]
CSV_PATH = ROOT / "docs" / "results" / "cross-library-current-20261125.csv"
TWEEN_PERFORMANCE_CSV = ROOT / "docs" / "results" / "tweenperformance-20261125.csv"
OUT_DIR = Path(__file__).resolve().parent

LIBRARIES = ["MyMotion", "LitMotion", "PrimeTween", "DOTween", "LeanTween", "FastTweener"]
CASES = [
    ("Float", 1000, "float × 1,000"),
    ("Float", 10000, "float × 10,000"),
    ("Float", 64000, "float × 64,000"),
    ("Transform", 1000, "Transform × 1,000"),
    ("Transform", 10000, "Transform × 10,000"),
]

# Highlight-and-context encoding: MyMotion in the accent blue, every other library in a
# neutral gray. Identity is carried by the row labels, so no legend is needed.
# Charts are drawn on a white background so they read the same on light and dark pages.
THEMES = {
    "light": {
        "accent": "#2a78d6",
        "context": "#8a8984",
        "text_primary": "#0b0b0b",
        "text_secondary": "#52514e",
        "rule": "#d8d7d2",
        "background": "#ffffff",
    },
}

FONT = "system-ui, -apple-system, 'Segoe UI', Helvetica, Arial, sans-serif"


def load():
    rows = defaultdict(list)
    with open(CSV_PATH, encoding="utf-8") as f:
        for r in csv.DictReader(f):
            rows[(r["scenario"], int(r["count"]), r["library"])].append(r)
    return rows


def median(rows, key, field, scale=1.0):
    values = [float(r[field]) * scale for r in rows[key]]
    return statistics.median(values)


def fmt(value, unit):
    if unit == "ms":
        return f"{value:,.2f} ms"
    if unit == "B":
        if value == 0:
            return "0 B"
        return f"{value:,.1f} B" if value < 10 else f"{value:,.0f} B"
    return f"{value:,.1f} ns"


def bar_path(x, y, w, h, r=4):
    """Bar anchored to the baseline at x, rounded only at the data end."""
    if w <= 0:
        return ""
    r = min(r, w, h / 2)
    return (
        f"M{x:.1f},{y:.1f} H{x + w - r:.1f} "
        f"A{r},{r} 0 0 1 {x + w:.1f},{y + r:.1f} V{y + h - r:.1f} "
        f"A{r},{r} 0 0 1 {x + w - r:.1f},{y + h:.1f} H{x:.1f} Z"
    )


def panel(out, theme, x0, y0, width, title, values, unit, order=None):
    t = THEMES[theme]
    label_w = 92
    value_w = 78
    bar_h = 16
    gap = 8
    out.append(
        f'<text x="{x0}" y="{y0 + 14}" font-size="14" font-weight="600" fill="{t["text_primary"]}">{escape(title)}</text>'
    )
    top = y0 + 28
    plot_x = x0 + label_w
    plot_w = width - label_w - value_w
    vmax = max(values.values()) or 1.0
    for i, lib in enumerate(order or LIBRARIES):
        y = top + i * (bar_h + gap)
        v = values[lib]
        w = plot_w * v / vmax
        is_mine = lib == "MyMotion"
        weight = "600" if is_mine else "400"
        ink = t["text_primary"] if is_mine else t["text_secondary"]
        out.append(
            f'<text x="{plot_x - 8}" y="{y + bar_h - 4}" font-size="12" text-anchor="end" font-weight="{weight}" fill="{ink}">{lib}</text>'
        )
        color = t["accent"] if is_mine else t["context"]
        if w > 0.5:
            out.append(f'<path d="{bar_path(plot_x, y, w, bar_h)}" fill="{color}"/>')
        out.append(
            f'<text x="{plot_x + max(w, 0) + 6}" y="{y + bar_h - 4}" font-size="12" font-weight="{weight}" fill="{ink}">{fmt(v, unit)}</text>'
        )
    # baseline
    bottom = top + len(LIBRARIES) * (bar_h + gap) - gap
    out.append(
        f'<line x1="{plot_x}" y1="{top - 4}" x2="{plot_x}" y2="{bottom + 4}" stroke="{t["rule"]}" stroke-width="1"/>'
    )
    return bottom - y0


def chart(theme, title, subtitle, panels, unit, columns=2, sort=True):
    t = THEMES[theme]
    width = 860
    col_w = (width - 24 * (columns - 1)) / columns
    out = []
    out.append(
        f'<text x="0" y="22" font-size="18" font-weight="700" fill="{t["text_primary"]}">{escape(title)}</text>'
    )
    out.append(f'<text x="0" y="44" font-size="13" fill="{t["text_secondary"]}">{escape(subtitle)}</text>')
    y = 64
    row_h = 0
    for i, (name, values) in enumerate(panels):
        col = i % columns
        if col == 0 and i > 0:
            y += row_h + 24
            row_h = 0
        order = sorted(values, key=lambda lib: values[lib]) if sort else None
        h = panel(out, theme, col * (col_w + 24), y, col_w, name, values, unit, order)
        row_h = max(row_h, h)
    height = int(y + row_h + 8)
    body = "\n    ".join(out)
    # Draw on a white background with padding so the chart reads the same on light and dark pages.
    pad_x, pad_y = 24, 20
    outer_w, outer_h = width + pad_x * 2, height + pad_y * 2
    return (
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{outer_w}" height="{outer_h}" viewBox="0 0 {outer_w} {outer_h}" '
        f'font-family="{FONT}" role="img" aria-label="{escape(title)}">\n  <title>{escape(title)}</title>\n'
        f'  <rect width="{outer_w}" height="{outer_h}" rx="8" fill="{t["background"]}"/>\n'
        f'  <g transform="translate({pad_x},{pad_y})">\n    {body}\n  </g>\n</svg>\n'
    )


def main():
    rows = load()
    update = [
        (label, {lib: median(rows, (s, c, lib), "update_net_ns_per_tween") for lib in LIBRARIES})
        for s, c, label in CASES
    ]
    create = [
        (label, {lib: median(rows, (s, c, lib), "create_ns_per_tween") for lib in LIBRARIES})
        for s, c, label in CASES
    ]
    # Managed allocation per tween: the largest per-workload median of each library.
    gc = {}
    for lib in LIBRARIES:
        gc[lib] = max(median(rows, (s, c, lib), "create_gc_bytes", 1.0 / c) for s, c, _ in CASES)

    for theme in THEMES:
        (OUT_DIR / f"benchmark-update.svg").write_text(
            chart(theme, "Update cost per tween", "Nanoseconds per tween per frame (lower is better). Release IL2CPP, median of 5 launches.", update, "ns"),
            encoding="utf-8",
        )
        (OUT_DIR / f"benchmark-create.svg").write_text(
            chart(theme, "Create cost per tween", "Nanoseconds to create and bind one tween (lower is better). Release IL2CPP, median of 5 launches.", create, "ns"),
            encoding="utf-8",
        )
        (OUT_DIR / f"benchmark-gc.svg").write_text(
            chart(theme, "Managed allocation on create", "Bytes allocated per tween (lower is better). Largest value across the five workloads.", [("All workloads", gc)], "B", columns=1),
            encoding="utf-8",
        )


def tween_performance():
    """Charts for the TweenPerformance reproduction (the benchmark shown in the LitMotion README)."""
    rows = defaultdict(list)
    with open(TWEEN_PERFORMANCE_CSV, encoding="utf-8") as f:
        for r in csv.DictReader(f):
            rows[(r["case"], r["library"])].append(float(r["value"]))
    cases = [
        ("startup_float", "Startup: 64,000 float tweens"),
        ("update_float", "Update: 64,000 float tweens"),
        ("startup_position", "Startup: 50,000 transform.position"),
        ("update_position", "Update: 50,000 transform.position"),
    ]
    panels = [(label, {lib: statistics.median(rows[(case, lib)]) for lib in LIBRARIES}) for case, label in cases]
    # GC.GetTotalMemory reports in heap blocks; values under 1 byte per tween (a few KB in total) are shown as 0.
    gc = {lib: statistics.median(rows[("gc_position", lib)]) for lib in LIBRARIES}
    gc = {lib: (0.0 if v < 1.0 else v) for lib, v in gc.items()}
    for theme in THEMES:
        (OUT_DIR / f"tweenperformance-time.svg").write_text(
            chart(theme, "TweenPerformance (LitMotion's benchmark)", "Milliseconds (lower is better). Release IL2CPP, Windows, median of 5 launches.", panels, "ms", sort=True),
            encoding="utf-8",
        )
        (OUT_DIR / f"tweenperformance-gc.svg").write_text(
            chart(theme, "TweenPerformance: allocation per position tween", "Bytes per tween (lower is better).", [("transform.position tween creation", gc)], "B", columns=1, sort=True),
            encoding="utf-8",
        )


LIFECYCLE_GLOB = "create-lifecycle-20261124/*.csv"


def comparisons():
    """MyMotion and LitMotion times for every measured test: (group, [(label, mymotion, litmotion, unit)])."""
    groups = []

    rows = defaultdict(list)
    with open(TWEEN_PERFORMANCE_CSV, encoding="utf-8") as f:
        for r in csv.DictReader(f):
            rows[(r["case"], r["library"])].append(float(r["value"]))

    def tp(case, label):
        return (label, statistics.median(rows[(case, "MyMotion")]), statistics.median(rows[(case, "LitMotion")]), "ms")

    groups.append(("LitMotion's benchmark (TweenPerformance)", [
        tp("startup_float", "Startup, 64,000 float"),
        tp("update_float", "Update, 64,000 float"),
        tp("startup_position", "Startup, 50,000 position"),
        tp("update_position", "Update, 50,000 position"),
    ]))

    per_tween = load()

    def pt(scenario, count, label, field):
        return (label, median(per_tween, (scenario, count, "MyMotion"), field), median(per_tween, (scenario, count, "LitMotion"), field), "ns")

    groups.append(("Update cost per tween", [pt(s_, c, label, "update_net_ns_per_tween") for s_, c, label in CASES]))
    groups.append(("Create cost per tween", [pt(s_, c, label, "create_ns_per_tween") for s_, c, label in CASES]))

    stages = defaultdict(lambda: ([], []))
    for path in sorted((ROOT / "docs" / "results").glob(LIFECYCLE_GLOB)):
        with open(path, encoding="utf-8") as f:
            values = {(r["library"], r["stage"]): float(r["ns_per_motion_median"]) for r in csv.DictReader(f)}
        for stage in ("builder", "bind", "bind_cancel_cleanup"):
            stages[stage][0].append(values[("mymotion", stage)])
            stages[stage][1].append(values[("litmotion", stage)])

    def lc(stage, label):
        my, lit = stages[stage]
        return (label, statistics.median(my), statistics.median(lit), "ns")

    groups.append(("Create to cleanup, per tween (4,096 tweens)", [
        lc("builder", "Configure (builder)"),
        lc("bind", "Configure + bind"),
        lc("bind_cancel_cleanup", "Bind + cancel + cleanup"),
    ]))
    return groups


def comparison_chart(theme, groups):
    """Paired bars per test: MyMotion (accent) above LitMotion (gray), each test on its own scale."""
    t = THEMES[theme]
    width = 860
    label_w = 190
    value_w = 80
    bar_h = 12
    pair_gap = 3
    row_gap = 12
    plot_x = label_w
    plot_w = width - label_w - value_w
    title = "MyMotion vs LitMotion: measured time"
    out = [
        f'<text x="0" y="22" font-size="18" font-weight="700" fill="{t["text_primary"]}">{title}</text>',
        f'<text x="0" y="44" font-size="13" fill="{t["text_secondary"]}">Lower is better. Each test is drawn on its own scale. Release IL2CPP, medians.</text>',
    ]
    # legend (two series)
    out.append(f'<rect x="0" y="58" width="12" height="12" rx="2" fill="{t["accent"]}"/>')
    out.append(f'<text x="18" y="68" font-size="12" font-weight="600" fill="{t["text_primary"]}">MyMotion</text>')
    out.append(f'<rect x="92" y="58" width="12" height="12" rx="2" fill="{t["context"]}"/>')
    out.append(f'<text x="110" y="68" font-size="12" fill="{t["text_secondary"]}">LitMotion</text>')
    y = 90
    first_top = y
    for group, items in groups:
        out.append(f'<text x="0" y="{y + 14}" font-size="14" font-weight="600" fill="{t["text_primary"]}">{escape(group)}</text>')
        y += 24
        for label, my, lit, unit in items:
            scale = plot_w / (max(my, lit) * 1.02)
            out.append(f'<text x="{plot_x - 8}" y="{y + bar_h + pair_gap / 2 + 4}" font-size="12" text-anchor="end" fill="{t["text_secondary"]}">{escape(label)}</text>')
            for k, (v, color, weight, ink) in enumerate((
                (my, t["accent"], "600", t["text_primary"]),
                (lit, t["context"], "400", t["text_secondary"]),
            )):
                by = y + k * (bar_h + pair_gap)
                w = v * scale
                out.append(f'<path d="{bar_path(plot_x, by, w, bar_h)}" fill="{color}"/>')
                text = f"{v:,.2f} ms" if unit == "ms" else f"{v:,.1f} ns"
                out.append(f'<text x="{plot_x + w + 6:.1f}" y="{by + bar_h - 2}" font-size="11" font-weight="{weight}" fill="{ink}">{text}</text>')
            y += 2 * bar_h + pair_gap + row_gap
        y += 10
    bottom = y - 10 - row_gap
    out.append(f'<line x1="{plot_x}" y1="{first_top}" x2="{plot_x}" y2="{bottom}" stroke="{t["rule"]}" stroke-width="1"/>')
    height = int(bottom + 8)
    body = "\n    ".join(out)
    pad_x, pad_y = 24, 20
    outer_w, outer_h = width + pad_x * 2, height + pad_y * 2
    return (
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{outer_w}" height="{outer_h}" viewBox="0 0 {outer_w} {outer_h}" '
        f'font-family="{FONT}" role="img" aria-label="{title}">\n'
        f'  <title>{title}</title>\n'
        f'  <rect width="{outer_w}" height="{outer_h}" rx="8" fill="{t["background"]}"/>\n'
        f'  <g transform="translate({pad_x},{pad_y})">\n    {body}\n  </g>\n</svg>\n'
    )


def comparison():
    groups = comparisons()
    for group, items in groups:
        print(group)
        for label, my, lit, unit in items:
            print(f"  {label}: MyMotion {my:.2f} {unit} / LitMotion {lit:.2f} {unit}")
    for theme in THEMES:
        (OUT_DIR / "mymotion-vs-litmotion.svg").write_text(comparison_chart(theme, groups), encoding="utf-8")


if __name__ == "__main__":
    tween_performance()
    main()
    comparison()
