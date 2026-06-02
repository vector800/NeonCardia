from pathlib import Path
from PIL import Image, ImageChops, ImageDraw, ImageFilter


SCRIPT_DIR = Path(__file__).resolve().parent
NEON_GRID_DIR = SCRIPT_DIR.parent
ASSETS_DIR = NEON_GRID_DIR.parents[2]

SOURCE_GRID = NEON_GRID_DIR / "Textures" / "BattleGrid_Full_Image2.png"
RANGE_ROWS_DIR = NEON_GRID_DIR / "RangeRows"
RANGE_PANELS_DIRS = [
    NEON_GRID_DIR / "RangePanels",
    NEON_GRID_DIR / "RangePanelsAligned",
]
SCREENSHOTS_DIR = ASSETS_DIR / "Screenshots"

ROWS = 3
COLUMNS = 6
SCALE = 4

# Bounds are the detected top-surface light footprint for each panel in
# BattleGrid_Full_Image2 pixel space. They intentionally do not use the
# board edge as the final-column right side.
PANEL_SURFACE_BOUNDS = {
    (0, 0): (207, 174, 405, 280),
    (0, 1): (434, 174, 618, 281),
    (0, 2): (652, 174, 823, 282),
    (0, 3): (860, 174, 1032, 281),
    (0, 4): (1066, 174, 1244, 281),
    (0, 5): (1273, 174, 1467, 280),
    (1, 0): (158, 308, 379, 436),
    (1, 1): (404, 307, 605, 437),
    (1, 2): (638, 307, 822, 437),
    (1, 3): (861, 306, 1042, 437),
    (1, 4): (1077, 306, 1271, 437),
    (1, 5): (1298, 306, 1514, 436),
    (2, 0): (102, 466, 342, 621),
    (2, 1): (366, 465, 586, 622),
    (2, 2): (619, 464, 819, 623),
    (2, 3): (861, 464, 1055, 622),
    (2, 4): (1092, 464, 1303, 622),
    (2, 5): (1330, 464, 1567, 623),
}


def is_neon_pixel(r, g, b, a):
    if a <= 12:
        return False

    hi = max(r, g, b)
    lo = min(r, g, b)
    return hi >= 115 and (hi - lo >= 32 or hi >= 210)


def convex_hull(points):
    points = sorted(set(points))
    if len(points) <= 1:
        return points

    def cross(o, a, b):
        return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0])

    lower = []
    for point in points:
        while len(lower) >= 2 and cross(lower[-2], lower[-1], point) <= 0:
            lower.pop()
        lower.append(point)

    upper = []
    for point in reversed(points):
        while len(upper) >= 2 and cross(upper[-2], upper[-1], point) <= 0:
            upper.pop()
        upper.append(point)

    return lower[:-1] + upper[:-1]


def beveled_fallback(bounds):
    left, top, right, bottom = bounds
    width = right - left
    height = bottom - top
    bevel_x = max(10, int(width * 0.13))
    bevel_y = max(8, int(height * 0.18))
    return [
        (left + bevel_x, top),
        (right - bevel_x, top),
        (right, top + bevel_y),
        (right, bottom - bevel_y),
        (right - bevel_x, bottom),
        (left + bevel_x, bottom),
        (left, bottom - bevel_y),
        (left, top + bevel_y),
    ]


def surface_hull_from_source(source, bounds):
    left, top, right, bottom = bounds
    pixels = source.load()
    points = []

    for y in range(top, bottom + 1):
        for x in range(left, right + 1):
            r, g, b, a = pixels[x, y]
            if is_neon_pixel(r, g, b, a):
                points.append((x, y))

    hull = convex_hull(points)
    if len(hull) < 4:
        return beveled_fallback(bounds)

    return hull


def draw_polygon_mask(size, polygon, bounds):
    hi_size = (size[0] * SCALE, size[1] * SCALE)
    hi_mask = Image.new("L", hi_size, 0)
    hi_draw = ImageDraw.Draw(hi_mask)
    scaled = [(int(x * SCALE), int(y * SCALE)) for x, y in polygon]
    hi_draw.polygon(scaled, fill=255)

    try:
        resample = Image.Resampling.LANCZOS
    except AttributeError:
        resample = Image.LANCZOS

    mask = hi_mask.resize(size, resample)
    left, top, right, bottom = bounds
    bounds_mask = Image.new("L", size, 0)
    bounds_draw = ImageDraw.Draw(bounds_mask)
    bounds_draw.rectangle([left, top, right, bottom], fill=255)
    return ImageChops.multiply(mask, bounds_mask)


def build_edge_mask(source, mask, bounds):
    edge = Image.new("L", source.size, 0)
    source_pixels = source.load()
    edge_pixels = edge.load()
    left, top, right, bottom = bounds

    for y in range(top, bottom + 1):
        for x in range(left, right + 1):
            r, g, b, a = source_pixels[x, y]
            if is_neon_pixel(r, g, b, a):
                hi = max(r, g, b)
                edge_pixels[x, y] = max(120, min(235, hi))

    edge = edge.filter(ImageFilter.MaxFilter(3))
    return ImageChops.multiply(edge, mask)


def tint_panel(source, mask, bounds):
    fill_alpha = mask.point(lambda a: int(a * 0.38))
    fill = Image.new("RGBA", source.size, (255, 176, 0, 0))
    fill.putalpha(fill_alpha)

    inner_glow_alpha = ImageChops.multiply(mask.filter(ImageFilter.GaussianBlur(5)), mask)
    inner_glow_alpha = inner_glow_alpha.point(lambda a: int(a * 0.30))
    inner_glow = Image.new("RGBA", source.size, (255, 214, 44, 0))
    inner_glow.putalpha(inner_glow_alpha)

    edge_alpha = build_edge_mask(source, mask, bounds)
    edge = Image.new("RGBA", source.size, (255, 226, 64, 0))
    edge.putalpha(edge_alpha)

    panel = Image.new("RGBA", source.size, (0, 0, 0, 0))
    panel.alpha_composite(fill)
    panel.alpha_composite(inner_glow)
    panel.alpha_composite(edge)
    return panel


def build_contact_sheet(source, row_images):
    label_height = 48
    gap = 36
    sheet = Image.new(
        "RGBA",
        (source.width, (source.height + label_height) * ROWS + gap * (ROWS - 1)),
        (0, 0, 0, 255),
    )
    draw = ImageDraw.Draw(sheet)

    y = 0
    for row, row_image in enumerate(row_images):
        draw.text((24, y + 14), f"Top-surface attack range mask R{row}: right edge clipped to panel top face", fill=(230, 238, 242, 255))
        preview = source.copy()
        preview.alpha_composite(row_image)
        sheet.alpha_composite(preview, (0, y + label_height))
        y += source.height + label_height + gap

    return sheet


def write_bounds_report(polygons, masks):
    lines = [
        "Top-surface attack range mask bounds",
        "",
        "All masks are generated from per-panel top-surface bright-pixel hulls.",
        "The last column never falls back to boardRight or texture.width.",
        "",
    ]
    for row in range(ROWS):
        for column in range(COLUMNS):
            mask_bbox = masks[(row, column)].getbbox()
            bounds = PANEL_SURFACE_BOUNDS[(row, column)]
            lines.append(
                f"R{row}C{column} source_bounds={bounds} mask_bbox={mask_bbox} hull_points={len(polygons[(row, column)])}"
            )

    (SCRIPT_DIR / "BattleGrid_AttackRangeTopSurfaceMasks.txt").write_text("\n".join(lines) + "\n", encoding="utf-8")


def main():
    source = Image.open(SOURCE_GRID).convert("RGBA")
    RANGE_ROWS_DIR.mkdir(parents=True, exist_ok=True)
    for directory in RANGE_PANELS_DIRS:
        directory.mkdir(parents=True, exist_ok=True)
    SCREENSHOTS_DIR.mkdir(parents=True, exist_ok=True)

    panel_images = {}
    masks = {}
    polygons = {}

    for row in range(ROWS):
        for column in range(COLUMNS):
            bounds = PANEL_SURFACE_BOUNDS[(row, column)]
            polygon = surface_hull_from_source(source, bounds)
            mask = draw_polygon_mask(source.size, polygon, bounds)
            panel = tint_panel(source, mask, bounds)

            polygons[(row, column)] = polygon
            masks[(row, column)] = mask
            panel_images[(row, column)] = panel

            for directory in RANGE_PANELS_DIRS:
                panel.save(directory / f"AttackRangePanel_R{row}_C{column}.png")

    row_images = []
    for row in range(ROWS):
        row_image = Image.new("RGBA", source.size, (0, 0, 0, 0))
        for column in range(COLUMNS):
            row_image.alpha_composite(panel_images[(row, column)])
        row_image.save(RANGE_ROWS_DIR / f"AttackRangeRow_R{row}.png")
        row_images.append(row_image)

    diagnostic = source.copy()
    all_panels = Image.new("RGBA", source.size, (0, 0, 0, 0))
    for panel in panel_images.values():
        all_panels.alpha_composite(panel)
    diagnostic.alpha_composite(all_panels)
    draw = ImageDraw.Draw(diagnostic)
    for row in range(ROWS):
        for column in range(COLUMNS):
            bounds = PANEL_SURFACE_BOUNDS[(row, column)]
            color = (0, 255, 120, 255) if column == COLUMNS - 1 else (0, 210, 90, 210)
            draw.rectangle(bounds, outline=color, width=2)
            polygon = polygons[(row, column)]
            if polygon:
                draw.line(polygon + [polygon[0]], fill=(255, 255, 255, 220), width=1)
    diagnostic.save(SCRIPT_DIR / "BattleGrid_AttackRangeTopSurfaceMasks_Diagnostic.png")

    contact_sheet = build_contact_sheet(source, row_images)
    contact_sheet.save(SCREENSHOTS_DIR / "BattleScene_attack_range_top_surface_masks_preview.png")

    write_bounds_report(polygons, masks)


if __name__ == "__main__":
    main()
