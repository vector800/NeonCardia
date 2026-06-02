from pathlib import Path
import math
from PIL import Image, ImageDraw, ImageFilter


SCRIPT_DIR = Path(__file__).resolve().parent
NEON_GRID_DIR = SCRIPT_DIR.parent
ASSETS_DIR = NEON_GRID_DIR.parents[2]
SOURCE_IMAGE_PATH = NEON_GRID_DIR / "Textures" / "BattleGrid_Full_Image2.png"

TEXTURE_SIZE = (1672, 941)
ROWS = 3
COLUMNS = 6
TRIM = 0.10
ROW_GAP_RATIOS = (0.020, 0.018, 0.017)
TILED_PANEL_WIDTH = 205.0
TILED_PANEL_HEIGHT = 108.0
TILED_PANEL_GAP_X = 24.0
TILED_PANEL_GAP_Y = 24.0
TILED_PANEL_TOP_LEFT = (
    (TEXTURE_SIZE[0] - (TILED_PANEL_WIDTH * COLUMNS + TILED_PANEL_GAP_X * (COLUMNS - 1))) * 0.5,
    204.0,
)
TILE_SPRITE_MARGIN = 36
PANEL_DEPTH_PIXELS = 18
PANEL_DEPTH_SHEAR_PIXELS = 4

UNIFIED_PANEL_DIR = NEON_GRID_DIR / "UnifiedPanels"
RANGE_ROWS_DIR = NEON_GRID_DIR / "RangeRows"
RANGE_PANELS_DIRS = (
    NEON_GRID_DIR / "RangePanels",
    NEON_GRID_DIR / "RangePanelsAligned",
)
SCREENSHOTS_DIR = ASSETS_DIR / "Screenshots"

# Four row strips, each defined as top-left, top-right, bottom-left, bottom-right
# in BattleGrid_Full_Image2 pixel space. The columns are evenly subdivided inside
# each row strip, so all gameplay cells come from one regularized PanelCell grid.
ROW_STRIPS = (
    ((205.0, 198.0), (1497.0, 201.0), (184.0, 292.0), (1518.0, 289.0)),
    ((168.0, 320.0), (1530.0, 322.0), (140.0, 431.0), (1534.0, 431.0)),
    ((123.0, 461.0), (1560.0, 458.0), (90.0, 588.0), (1589.0, 589.0)),
)

# Source-measured panel corners from BattleGrid_Full_Image2.png. These keep the
# individual rotations visible in the original board art.
RAW_PANEL_CORNERS = (
    ((205.0, 198.0), (386.0, 197.0), (367.0, 292.0), (184.0, 292.0)),
    ((422.0, 198.0), (598.0, 197.0), (592.0, 293.0), (409.0, 291.0)),
    ((643.0, 199.0), (810.0, 197.0), (814.0, 293.0), (630.0, 291.0)),
    ((863.0, 199.0), (1037.0, 198.0), (1057.0, 288.0), (867.0, 294.0)),
    ((1080.0, 200.0), (1266.0, 198.0), (1261.0, 291.0), (1077.0, 292.0)),
    ((1297.0, 197.0), (1497.0, 201.0), (1518.0, 289.0), (1294.0, 293.0)),
    ((168.0, 320.0), (369.0, 320.0), (356.0, 429.0), (140.0, 431.0)),
    ((398.0, 321.0), (592.0, 321.0), (595.0, 428.0), (380.0, 431.0)),
    ((628.0, 321.0), (813.0, 321.0), (811.0, 430.0), (618.0, 430.0)),
    ((863.0, 320.0), (1052.0, 321.0), (1061.0, 429.0), (854.0, 430.0)),
    ((1087.0, 320.0), (1277.0, 320.0), (1296.0, 430.0), (1094.0, 430.0)),
    ((1320.0, 318.0), (1530.0, 322.0), (1534.0, 431.0), (1320.0, 429.0)),
    ((123.0, 461.0), (337.0, 459.0), (337.0, 586.0), (90.0, 588.0)),
    ((365.0, 461.0), (585.0, 459.0), (571.0, 587.0), (348.0, 589.0)),
    ((611.0, 459.0), (810.0, 461.0), (811.0, 587.0), (600.0, 588.0)),
    ((865.0, 458.0), (1066.0, 458.0), (1072.0, 588.0), (853.0, 588.0)),
    ((1097.0, 457.0), (1306.0, 458.0), (1327.0, 589.0), (1088.0, 586.0)),
    ((1331.0, 457.0), (1560.0, 458.0), (1589.0, 589.0), (1316.0, 586.0)),
)
SELECTED_GEOMETRY_NAME = "tiled_grid"


def lerp(a, b, t):
    return (a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t)


def polygon_center(points):
    return (
        sum(point[0] for point in points) / len(points),
        sum(point[1] for point in points) / len(points),
    )


def regularized_cell_corners(row, column):
    top_left, top_right, bottom_left, bottom_right = ROW_STRIPS[row]
    gap = ROW_GAP_RATIOS[row]
    cell = (1.0 - gap * (COLUMNS - 1)) / COLUMNS
    start = column * (cell + gap)
    end = start + cell
    return (
        lerp(top_left, top_right, start),
        lerp(top_left, top_right, end),
        lerp(bottom_left, bottom_right, end),
        lerp(bottom_left, bottom_right, start),
    )


def tiled_grid_cell_corners(row, column):
    x = TILED_PANEL_TOP_LEFT[0] + column * (TILED_PANEL_WIDTH + TILED_PANEL_GAP_X)
    y = TILED_PANEL_TOP_LEFT[1] + row * (TILED_PANEL_HEIGHT + TILED_PANEL_GAP_Y)
    return (
        (x, y),
        (x + TILED_PANEL_WIDTH, y),
        (x + TILED_PANEL_WIDTH, y + TILED_PANEL_HEIGHT),
        (x, y + TILED_PANEL_HEIGHT),
    )


def raw_cell_corners(row, column):
    return RAW_PANEL_CORNERS[row * COLUMNS + column]


def blend_corners(a, b, amount):
    return tuple(lerp(a[i], b[i], amount) for i in range(4))


def build_regularized_geometry():
    return {
        (row, column): regularized_cell_corners(row, column)
        for row in range(ROWS)
        for column in range(COLUMNS)
    }


def build_tiled_grid_geometry():
    return {
        (row, column): tiled_grid_cell_corners(row, column)
        for row in range(ROWS)
        for column in range(COLUMNS)
    }


def build_raw_geometry():
    return {
        (row, column): raw_cell_corners(row, column)
        for row in range(ROWS)
        for column in range(COLUMNS)
    }


def build_blended_geometry(amount):
    regularized = build_regularized_geometry()
    raw = build_raw_geometry()
    return {
        key: blend_corners(regularized[key], raw[key], amount)
        for key in regularized
    }


def build_row_average_shape_geometry():
    regularized = build_regularized_geometry()
    raw = build_raw_geometry()
    geometry = {}
    for row in range(ROWS):
        offsets = [(0.0, 0.0) for _ in range(4)]
        for column in range(COLUMNS):
            corners = raw[(row, column)]
            center = polygon_center(corners)
            offsets = [
                (offsets[i][0] + corners[i][0] - center[0], offsets[i][1] + corners[i][1] - center[1])
                for i in range(4)
            ]

        offsets = [(point[0] / COLUMNS, point[1] / COLUMNS) for point in offsets]
        for column in range(COLUMNS):
            center = polygon_center(regularized[(row, column)])
            geometry[(row, column)] = tuple((center[0] + offsets[i][0], center[1] + offsets[i][1]) for i in range(4))
    return geometry


def build_candidate_geometries():
    return {
        "tiled_grid": build_tiled_grid_geometry(),
        "regularized": build_regularized_geometry(),
        "row_average_shape": build_row_average_shape_geometry(),
        "blend_raw_50": build_blended_geometry(0.50),
        "blend_raw_75": build_blended_geometry(0.75),
        "blend_raw_90": build_blended_geometry(0.90),
        "source_raw": build_raw_geometry(),
    }


def beveled_polygon(corners, trim=TRIM):
    top_left, top_right, bottom_right, bottom_left = corners
    return (
        lerp(top_left, top_right, trim),
        lerp(top_left, top_right, 1.0 - trim),
        lerp(top_right, bottom_right, trim),
        lerp(top_right, bottom_right, 1.0 - trim),
        lerp(bottom_right, bottom_left, trim),
        lerp(bottom_right, bottom_left, 1.0 - trim),
        lerp(bottom_left, top_left, trim),
        lerp(bottom_left, top_left, 1.0 - trim),
    )


def rounded_points(points):
    return [(int(round(x)), int(round(y))) for x, y in points]


def offset_points(points, dx, dy):
    return tuple((point[0] + dx, point[1] + dy) for point in points)


def multiply_rgb(color, amount, alpha=None):
    return (
        max(0, min(255, int(color[0] * amount))),
        max(0, min(255, int(color[1] * amount))),
        max(0, min(255, int(color[2] * amount))),
        color[3] if alpha is None and len(color) > 3 else (alpha if alpha is not None else 255),
    )


def draw_panel_art(row, column, corners):
    is_ally = column < 3
    fill_color = (58, 16, 11, 205) if is_ally else (8, 29, 52, 205)
    glow_color = (255, 67, 35, 190) if is_ally else (35, 193, 255, 190)
    edge_color = (255, 92, 52, 245) if is_ally else (70, 220, 255, 245)
    hot_color = (255, 246, 160, 240) if is_ally else (170, 246, 255, 230)
    circuit_color = (255, 73, 35, 155) if is_ally else (36, 188, 255, 155)
    side_front_color = multiply_rgb(edge_color, 0.30, 205)
    side_dark_color = multiply_rgb(edge_color, 0.15, 185)
    side_hot_color = multiply_rgb(edge_color, 0.86, 210)

    size = TEXTURE_SIZE
    top_polygon = beveled_polygon(corners, 0.11)
    points = rounded_points(top_polygon)
    depth_polygon = offset_points(top_polygon, PANEL_DEPTH_SHEAR_PIXELS, PANEL_DEPTH_PIXELS)
    depth_points = rounded_points(depth_polygon)
    mask = Image.new("L", size, 0)
    mask_draw = ImageDraw.Draw(mask)
    mask_draw.polygon(points, fill=255)

    panel = Image.new("RGBA", size, (0, 0, 0, 0))
    shadow = Image.new("RGBA", size, (0, 0, 0, 0))
    shadow_draw = ImageDraw.Draw(shadow)
    shadow_draw.polygon(rounded_points(offset_points(depth_polygon, 7, 8)), fill=(0, 0, 0, 135))
    shadow = shadow.filter(ImageFilter.GaussianBlur(7))
    panel.alpha_composite(shadow)

    side = Image.new("RGBA", size, (0, 0, 0, 0))
    side_draw = ImageDraw.Draw(side)
    front_side = [
        points[3], points[4], points[5], points[6],
        depth_points[6], depth_points[5], depth_points[4], depth_points[3],
    ]
    right_side = [points[2], points[3], depth_points[3], depth_points[2]]
    left_side = [points[6], points[7], depth_points[7], depth_points[6]]
    side_draw.polygon(right_side, fill=side_dark_color)
    side_draw.polygon(left_side, fill=side_dark_color)
    side_draw.polygon(front_side, fill=side_front_color)
    side_draw.line(
        [points[3], points[4], points[5], points[6]],
        fill=multiply_rgb(side_hot_color, 0.78, 180),
        width=3,
        joint="curve",
    )
    side_draw.line(
        [depth_points[3], depth_points[4], depth_points[5], depth_points[6]],
        fill=side_hot_color,
        width=3,
        joint="curve",
    )
    side_draw.line([points[2], depth_points[2]], fill=(side_hot_color[0], side_hot_color[1], side_hot_color[2], 115), width=2)
    side_draw.line([points[7], depth_points[7]], fill=(side_hot_color[0], side_hot_color[1], side_hot_color[2], 95), width=2)
    side_glow = side.filter(ImageFilter.GaussianBlur(4))
    side_glow.putalpha(side_glow.getchannel("A").point(lambda a: int(a * 0.55)))
    panel.alpha_composite(side_glow)
    panel.alpha_composite(side)

    glow = Image.new("RGBA", size, (0, 0, 0, 0))
    glow_draw = ImageDraw.Draw(glow)
    for width, alpha in ((22, 42), (13, 76), (6, 140)):
        color = (glow_color[0], glow_color[1], glow_color[2], alpha)
        glow_draw.line(points + [points[0]], fill=color, width=width, joint="curve")
    panel.alpha_composite(glow)

    fill = Image.new("RGBA", size, fill_color)
    fill.putalpha(mask.point(lambda a: int(a * 0.72)))
    panel.alpha_composite(fill)

    edge = Image.new("RGBA", size, (0, 0, 0, 0))
    edge_draw = ImageDraw.Draw(edge)
    edge_draw.line(points + [points[0]], fill=multiply_rgb(edge_color, 0.74, 230), width=7, joint="curve")
    edge_draw.line(points + [points[0]], fill=edge_color, width=4, joint="curve")

    inner = rounded_points(beveled_polygon(inset_corners(corners, 0.13), 0.16))
    edge_draw.line(inner + [inner[0]], fill=hot_color, width=4, joint="curve")
    edge_draw.line(inner + [inner[0]], fill=(hot_color[0], hot_color[1], hot_color[2], 95), width=10, joint="curve")

    cx, cy = polygon_center(corners)
    left_mid = lerp(corners[3], corners[0], 0.52)
    right_mid = lerp(corners[2], corners[1], 0.50)
    top_mid = lerp(corners[0], corners[1], 0.52)
    bottom_mid = lerp(corners[3], corners[2], 0.54)
    circuit_a = [
        (left_mid[0] + 38, left_mid[1] - 2),
        (cx - 42, cy - 2),
        (cx - 10, cy - 20),
        (right_mid[0] - 44, right_mid[1] - 20),
    ]
    circuit_b = [
        (top_mid[0] - 62, top_mid[1] + 34),
        (cx + 18, cy + 30),
        (cx + 42, cy + 12),
        (right_mid[0] - 34, right_mid[1] + 10),
    ]
    circuit_c = [
        (bottom_mid[0] - 78, bottom_mid[1] - 22),
        (cx - 55, cy + 10),
        (cx - 18, cy + 10),
    ]
    for circuit in (circuit_a, circuit_b, circuit_c):
        edge_draw.line(rounded_points(circuit), fill=circuit_color, width=2)
        for px, py in rounded_points(circuit[1:-1]):
            edge_draw.ellipse((px - 2, py - 2, px + 2, py + 2), fill=circuit_color)

    edge.putalpha(Image.composite(edge.getchannel("A"), Image.new("L", size, 0), mask))
    panel.alpha_composite(edge)
    return panel


def inset_corners(corners, amount):
    center = polygon_center(corners)
    return tuple(lerp(point, center, amount) for point in corners)


def draw_range_panel(corners):
    size = TEXTURE_SIZE
    points = rounded_points(beveled_polygon(corners, 0.11))
    mask = Image.new("L", size, 0)
    mask_draw = ImageDraw.Draw(mask)
    mask_draw.polygon(points, fill=255)

    panel = Image.new("RGBA", size, (0, 0, 0, 0))
    fill = Image.new("RGBA", size, (255, 177, 0, 0))
    fill.putalpha(mask.point(lambda a: int(a * 0.42)))
    panel.alpha_composite(fill)

    halo_mask = mask.filter(ImageFilter.GaussianBlur(5))
    halo = Image.new("RGBA", size, (255, 214, 48, 0))
    halo.putalpha(halo_mask.point(lambda a: int(a * 0.24)))
    panel.alpha_composite(halo)

    edge = Image.new("RGBA", size, (0, 0, 0, 0))
    edge_draw = ImageDraw.Draw(edge)
    edge_draw.line(points + [points[0]], fill=(255, 236, 90, 235), width=5, joint="curve")
    inner = rounded_points(beveled_polygon(inset_corners(corners, 0.12), 0.14))
    edge_draw.line(inner + [inner[0]], fill=(255, 241, 110, 180), width=3, joint="curve")
    panel.alpha_composite(edge)
    return panel


def draw_candidate_line_mask(corners_by_cell, width=6):
    mask = Image.new("L", TEXTURE_SIZE, 0)
    draw = ImageDraw.Draw(mask)
    for row in range(ROWS):
        for column in range(COLUMNS):
            corners = corners_by_cell[(row, column)]
            outer = rounded_points(beveled_polygon(corners, 0.11))
            inner = rounded_points(beveled_polygon(inset_corners(corners, 0.13), 0.16))
            draw.line(outer + [outer[0]], fill=255, width=width, joint="curve")
            draw.line(inner + [inner[0]], fill=255, width=max(2, width // 2), joint="curve")
    return mask


def build_source_bright_masks():
    source = Image.open(SOURCE_IMAGE_PATH).convert("RGBA")
    pixels = source.load()
    bright = Image.new("L", TEXTURE_SIZE, 0)
    bright_pixels = bright.load()
    luma = Image.new("L", TEXTURE_SIZE, 0)
    luma_pixels = luma.load()
    for y in range(TEXTURE_SIZE[1]):
        for x in range(TEXTURE_SIZE[0]):
            r, g, b, a = pixels[x, y]
            value = int(r * 0.299 + g * 0.587 + b * 0.114)
            luma_pixels[x, y] = value
            if a > 16 and max(r, g, b) > 145 and value > 55:
                bright_pixels[x, y] = 255
    return source, bright, bright.filter(ImageFilter.MaxFilter(7)), bright.filter(ImageFilter.MaxFilter(15)), luma


def count_overlap(mask_a, mask_b):
    a = mask_a.tobytes()
    b = mask_b.tobytes()
    hits = 0
    total = 0
    for av, bv in zip(a, b):
        if av:
            total += 1
            if bv:
                hits += 1
    return hits, total


def mean_luma_under_mask(luma, mask):
    luma_bytes = luma.tobytes()
    mask_bytes = mask.tobytes()
    total = 0
    count = 0
    for lv, mv in zip(luma_bytes, mask_bytes):
        if mv:
            total += lv
            count += 1
    return total / count if count else 0.0


def top_edge_angle(corners):
    top_left, top_right = corners[0], corners[1]
    return math.degrees(math.atan2(top_right[1] - top_left[1], top_right[0] - top_left[0]))


def evaluate_candidate(name, corners_by_cell, bright_near, bright_far, luma):
    line_mask = draw_candidate_line_mask(corners_by_cell)
    near_hits, line_pixels = count_overlap(line_mask, bright_near)
    far_hits, _ = count_overlap(line_mask, bright_far)
    near_ratio = near_hits / line_pixels if line_pixels else 0.0
    far_ratio = far_hits / line_pixels if line_pixels else 0.0
    luma_score = mean_luma_under_mask(luma, line_mask) / 255.0
    score = near_ratio * 0.58 + far_ratio * 0.24 + luma_score * 0.18
    return {
        "name": name,
        "score": score,
        "near_ratio": near_ratio,
        "far_ratio": far_ratio,
        "luma_score": luma_score,
        "line_pixels": line_pixels,
        "line_mask": line_mask,
    }


def evaluate_candidate_geometries(candidates):
    source, bright, bright_near, bright_far, luma = build_source_bright_masks()
    results = []
    for name, corners_by_cell in candidates.items():
        results.append(evaluate_candidate(name, corners_by_cell, bright_near, bright_far, luma))
    results.sort(key=lambda item: item["score"], reverse=True)
    return source, bright, results


def create_rotation_candidate_sheet(source, candidates, results):
    thumb_size = (418, 235)
    rows = len(results)
    sheet = Image.new("RGBA", (thumb_size[0] * 2, thumb_size[1] * rows), (4, 7, 10, 255))
    for index, result in enumerate(results):
        name = result["name"]
        preview = source.copy()
        line_rgba = Image.new("RGBA", TEXTURE_SIZE, (255, 225, 48, 0))
        line_rgba.putalpha(result["line_mask"].point(lambda value: 210 if value else 0))
        preview.alpha_composite(line_rgba)
        preview = preview.resize(thumb_size, Image.Resampling.LANCZOS)

        structure = Image.new("RGBA", TEXTURE_SIZE, (4, 7, 10, 255))
        draw = ImageDraw.Draw(structure)
        for row in range(ROWS):
            for column in range(COLUMNS):
                points = rounded_points(beveled_polygon(candidates[name][(row, column)], 0.11))
                color = (255, 74, 40, 255) if column < 3 else (40, 190, 255, 255)
                draw.line(points + [points[0]], fill=color, width=5, joint="curve")
        structure = structure.resize(thumb_size, Image.Resampling.LANCZOS)

        y = index * thumb_size[1]
        sheet.alpha_composite(preview, (0, y))
        sheet.alpha_composite(structure, (thumb_size[0], y))
        label = (
            f"{index + 1}. {name}  score={result['score']:.4f} "
            f"near={result['near_ratio']:.3f} far={result['far_ratio']:.3f}"
        )
        ImageDraw.Draw(sheet).text((8, y + 8), label, fill=(245, 250, 255, 255))
    return sheet


def write_candidate_report(results, candidates, selected_name):
    lines = [
        "BattleScene panel rotation candidate report",
        "",
        "Scoring overlays each candidate's generated panel edge lines on BattleGrid_Full_Image2.png.",
        "near/far are overlap ratios against dilated bright source edges; luma is source brightness under candidate lines.",
        f"selected: {selected_name}",
        "",
        "rank,name,score,near,far,luma,line_pixels,row0_top_angles,row1_top_angles,row2_top_angles",
    ]
    for rank, result in enumerate(results, 1):
        geometry = candidates[result["name"]]
        angle_groups = []
        for row in range(ROWS):
            angles = [top_edge_angle(geometry[(row, column)]) for column in range(COLUMNS)]
            angle_groups.append("/".join(f"{angle:.2f}" for angle in angles))
        lines.append(
            f"{rank},{result['name']},{result['score']:.6f},{result['near_ratio']:.6f},"
            f"{result['far_ratio']:.6f},{result['luma_score']:.6f},{result['line_pixels']},"
            + ",".join(angle_groups)
        )
    (SCRIPT_DIR / "BattleScenePanelRotationCandidateReport.csv").write_text("\n".join(lines) + "\n", encoding="utf-8")


def create_board_frame(corners_by_cell):
    frame = Image.new("RGBA", TEXTURE_SIZE, (0, 0, 0, 0))
    xs = [point[0] for corners in corners_by_cell.values() for point in corners]
    ys = [point[1] for corners in corners_by_cell.values() for point in corners]
    left = min(xs) - 34
    top = min(ys) - 28
    right = max(xs) + 34
    bottom = max(ys) + 56
    draw = ImageDraw.Draw(frame)

    shadow = Image.new("RGBA", TEXTURE_SIZE, (0, 0, 0, 0))
    shadow_draw = ImageDraw.Draw(shadow)
    shadow_draw.rounded_rectangle((left, top + 12, right, bottom + 20), radius=24, fill=(0, 0, 0, 120))
    shadow = shadow.filter(ImageFilter.GaussianBlur(12))
    frame.alpha_composite(shadow)

    draw.rounded_rectangle((left, top + 20, right, bottom), radius=22, fill=(2, 7, 11, 230), outline=(23, 42, 53, 190), width=4)
    draw.rounded_rectangle((left, top, right, bottom - 18), radius=22, fill=(5, 12, 18, 218), outline=(54, 77, 86, 170), width=3)
    split_x = (corners_by_cell[(0, 2)][1][0] + corners_by_cell[(0, 3)][0][0]) * 0.5
    draw.line((split_x, top + 6, split_x, bottom - 24), fill=(38, 82, 116, 210), width=4)

    ally_left = corners_by_cell[(0, 0)][0][0] - 16
    ally_right = corners_by_cell[(0, 2)][1][0] + 16
    enemy_left = corners_by_cell[(0, 3)][0][0] - 16
    enemy_right = corners_by_cell[(0, 5)][1][0] + 16
    rail_y = bottom - 24
    draw.line((ally_left, rail_y, ally_right, rail_y), fill=(255, 54, 28, 230), width=6)
    draw.line((enemy_left, rail_y, enemy_right, rail_y), fill=(36, 184, 255, 230), width=6)
    draw.line((ally_left, rail_y + 12, ally_right, rail_y + 12), fill=(255, 54, 28, 125), width=3)
    draw.line((enemy_left, rail_y + 12, enemy_right, rail_y + 12), fill=(36, 184, 255, 125), width=3)
    draw.line((ally_left, top + 18, ally_right, top + 18), fill=(255, 54, 28, 120), width=3)
    draw.line((enemy_left, top + 18, enemy_right, top + 18), fill=(36, 184, 255, 120), width=3)

    return frame


def create_source_cutout_board_frame(corners_by_cell):
    if SOURCE_IMAGE_PATH.exists():
        frame = Image.open(SOURCE_IMAGE_PATH).convert("RGBA")
    else:
        return create_board_frame(corners_by_cell)

    clear_mask = Image.new("L", TEXTURE_SIZE, 0)
    mask_draw = ImageDraw.Draw(clear_mask)
    for row in range(ROWS):
        for column in range(COLUMNS):
            corners = inset_corners(corners_by_cell[(row, column)], -0.035)
            mask_draw.polygon(rounded_points(beveled_polygon(corners, 0.08)), fill=255)

    alpha = frame.getchannel("A")
    alpha = Image.composite(Image.new("L", TEXTURE_SIZE, 0), alpha, clear_mask)
    frame.putalpha(alpha)
    return frame


def save_tile_sprites(panel_images_by_cell, range_panel_images):
    row = 1
    ally_column = 1
    enemy_column = 4

    def crop_cell(image, column):
        corners = tiled_grid_cell_corners(row, column)
        xs = [point[0] for point in corners]
        ys = [point[1] for point in corners]
        crop = (
            int(round(min(xs) - TILE_SPRITE_MARGIN)),
            int(round(min(ys) - TILE_SPRITE_MARGIN)),
            int(round(max(xs) + TILE_SPRITE_MARGIN)),
            int(round(max(ys) + TILE_SPRITE_MARGIN)),
        )
        return image.crop(crop)

    crop_cell(panel_images_by_cell[(row, ally_column)], ally_column).save(UNIFIED_PANEL_DIR / "TilePanel_Ally.png")
    crop_cell(panel_images_by_cell[(row, enemy_column)], enemy_column).save(UNIFIED_PANEL_DIR / "TilePanel_Enemy.png")
    crop_cell(range_panel_images[(row, enemy_column)], enemy_column).save(UNIFIED_PANEL_DIR / "TilePanel_Range.png")


def create_contact_sheet(panel_images, range_rows, board_frame):
    width, height = TEXTURE_SIZE
    sheet = Image.new("RGBA", (width, height * 2 + 96), (4, 7, 10, 255))
    draw = ImageDraw.Draw(sheet)
    draw.text((24, 18), "Unified PanelCell grid: 18 independent panel PNGs", fill=(235, 245, 255, 255))
    board = Image.new("RGBA", TEXTURE_SIZE, (4, 7, 10, 255))
    board.alpha_composite(board_frame)
    for image in panel_images:
        board.alpha_composite(image)
    sheet.alpha_composite(board, (0, 48))
    draw.text((24, height + 62), "Unified range masks: generated from the same PanelCell polygons", fill=(235, 245, 255, 255))
    range_preview = board.copy()
    range_preview.alpha_composite(range_rows[1])
    sheet.alpha_composite(range_preview, (0, height + 96))
    return sheet


def create_tiled_runtime_preview(corners_by_cell, board_frame):
    preview = Image.new("RGBA", TEXTURE_SIZE, (4, 7, 10, 255))
    preview.alpha_composite(board_frame)
    ally_tile = Image.open(UNIFIED_PANEL_DIR / "TilePanel_Ally.png").convert("RGBA")
    enemy_tile = Image.open(UNIFIED_PANEL_DIR / "TilePanel_Enemy.png").convert("RGBA")
    for row in range(ROWS):
        for column in range(COLUMNS):
            tile = ally_tile if column < 3 else enemy_tile
            center = polygon_center(corners_by_cell[(row, column)])
            top_left = (int(round(center[0] - tile.width * 0.5)), int(round(center[1] - tile.height * 0.5)))
            preview.alpha_composite(tile, top_left)
    return preview


def write_csharp_geometry(corners_by_cell):
    write_csharp_geometry_file(
        corners_by_cell,
        "BattleSceneUnifiedPanelCornerPixels",
        SCRIPT_DIR / "BattleSceneUnifiedPanelCornerPixels.generated.txt")


def write_visual_csharp_geometry(corners_by_cell):
    write_csharp_geometry_file(
        corners_by_cell,
        "BattleSceneVisualPanelCornerPixels",
        SCRIPT_DIR / "BattleSceneVisualPanelCornerPixels.generated.txt")


def write_csharp_geometry_file(corners_by_cell, field_name, output_path):
    lines = []
    lines.append(f"private static readonly Vector2[][] {field_name} =")
    lines.append("{")
    for row in range(ROWS):
        for column in range(COLUMNS):
            corners = corners_by_cell[(row, column)]
            values = ", ".join(
                f"new Vector2({point[0]:.1f}f, {point[1]:.1f}f)"
                for point in corners
            )
            comma = "," if row != ROWS - 1 or column != COLUMNS - 1 else ""
            lines.append(f"    new[] {{ {values} }}{comma}")
    lines.append("};")
    output_path.write_text("\n".join(lines) + "\n", encoding="utf-8")


def write_report(corners_by_cell):
    lines = [
        "Unified PanelCell geometry",
        "",
        "All panel visuals, range masks, and runtime colliders use these same selected cell corners.",
        f"Selected rotation candidate: {SELECTED_GEOMETRY_NAME}",
        "",
    ]
    for row in range(ROWS):
        areas = []
        for column in range(COLUMNS):
            corners = corners_by_cell[(row, column)]
            xs = [point[0] for point in corners]
            ys = [point[1] for point in corners]
            areas.append((max(xs) - min(xs)) * (max(ys) - min(ys)))
        lines.append(f"row {row} bbox area spread: min={min(areas):.1f} max={max(areas):.1f} delta={max(areas)-min(areas):.1f}")
    (SCRIPT_DIR / "BattleSceneUnifiedPanelCells.txt").write_text("\n".join(lines) + "\n", encoding="utf-8")


def main():
    UNIFIED_PANEL_DIR.mkdir(parents=True, exist_ok=True)
    RANGE_ROWS_DIR.mkdir(parents=True, exist_ok=True)
    for directory in RANGE_PANELS_DIRS:
        directory.mkdir(parents=True, exist_ok=True)
    SCREENSHOTS_DIR.mkdir(parents=True, exist_ok=True)

    panel_images = []
    panel_images_by_cell = {}
    range_panel_images = {}
    candidates = build_candidate_geometries()
    source, bright, results = evaluate_candidate_geometries(candidates)
    best_name = results[0]["name"]
    selected_name = SELECTED_GEOMETRY_NAME if SELECTED_GEOMETRY_NAME in candidates else best_name

    corners_by_cell = candidates[selected_name]
    create_rotation_candidate_sheet(source, candidates, results).save(SCREENSHOTS_DIR / "BattleScene_panel_rotation_candidates.png")
    write_candidate_report(results, candidates, selected_name)

    for row in range(ROWS):
        for column in range(COLUMNS):
            corners = corners_by_cell[(row, column)]
            panel = draw_panel_art(row, column, corners)
            panel_images.append(panel)
            panel_images_by_cell[(row, column)] = panel
            panel.save(UNIFIED_PANEL_DIR / f"UnifiedPanel_R{row}_C{column}.png")

            range_panel = draw_range_panel(corners)
            range_panel_images[(row, column)] = range_panel
            for directory in RANGE_PANELS_DIRS:
                range_panel.save(directory / f"AttackRangePanel_R{row}_C{column}.png")

    range_rows = []
    for row in range(ROWS):
        row_image = Image.new("RGBA", TEXTURE_SIZE, (0, 0, 0, 0))
        for column in range(COLUMNS):
            row_image.alpha_composite(range_panel_images[(row, column)])
        row_image.save(RANGE_ROWS_DIR / f"AttackRangeRow_R{row}.png")
        range_rows.append(row_image)

    board_frame = create_board_frame(corners_by_cell)
    board_frame.save(UNIFIED_PANEL_DIR / "UnifiedBoardFrame.png")
    save_tile_sprites(panel_images_by_cell, range_panel_images)
    create_contact_sheet(panel_images, range_rows, board_frame).save(SCREENSHOTS_DIR / "BattleScene_unified_panel_cells_preview.png")
    create_tiled_runtime_preview(corners_by_cell, board_frame).save(SCREENSHOTS_DIR / "BattleScene_tiled_runtime_preview.png")
    write_csharp_geometry(corners_by_cell)
    write_visual_csharp_geometry(corners_by_cell)
    write_report(corners_by_cell)


if __name__ == "__main__":
    main()
