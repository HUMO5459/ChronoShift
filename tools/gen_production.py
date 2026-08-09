#!/usr/bin/env python3
"""Generates the resource / production content and patches the existing campaign
data: items, recipes, per-map deposits + workshop, per-task equipment gates and
calendar jumps, per-mission era years.

Re-runnable: every patch is idempotent (it strips its own previous output first).
"""
import math
import os
import random
import re

ROOT = "/Users/humoyunochilov/PROJECTS/ChronoShift"
RES = os.path.join(ROOT, "resources", "data")
ITEMS = os.path.join(RES, "items")
RECIPES = os.path.join(RES, "recipes")
MAPS = os.path.join(RES, "maps")
TASKS = os.path.join(RES, "tasks")
MISSIONS = os.path.join(RES, "missions")

ITEM_SCRIPT = "res://scripts/data/ItemData.cs"
RECIPE_SCRIPT = "res://scripts/data/RecipeData.cs"

# --- item catalog -----------------------------------------------------------
# id, uid, name, short, category, tint, description
ITEM_DEFS = [
    ("res_wood",  "woeitm01", "Yog'och", "YOG'OCH", 0, (0.55, 0.38, 0.20),
     "O'rmondan kesilgan xom yog'och. Ko'prik, istehkom va yordamchi qurilmalar uchun asos."),
    ("res_iron",  "woeitm02", "Temir",   "TEMIR",   0, (0.62, 0.65, 0.70),
     "Konda qazilgan temir rudasi. Har qanday jiddiy uskunaning o'zagi."),
    ("res_coal",  "woeitm03", "Ko'mir",  "KO'MIR",  0, (0.16, 0.16, 0.18),
     "Yoqilg'i. Ko'mirsiz na pech, na bug' mashinasi ishlaydi."),
    ("res_stone", "woeitm04", "Tosh",    "TOSH",    0, (0.52, 0.52, 0.49),
     "Karyerdan olingan tosh. Mustahkam istehkom va poydevor uchun."),

    ("eq_repair",   "woeitm05", "Ta'mirlash to'plami",     "", 1, (0.85, 0.62, 0.28),
     "Ehtiyot qismlar, payvand va asboblar. Ishdan chiqqan uskunani tiklaydi."),
    ("eq_install",  "woeitm06", "O'rnatish komplekti",     "", 1, (0.40, 0.62, 0.80),
     "Kabel, mahkamlagich va o'lchov asboblari. Yangi qurilmani joyiga o'rnatadi."),
    ("eq_bridge",   "woeitm07", "Ponton to'plami",         "", 1, (0.45, 0.55, 0.42),
     "Suzuvchi seksiyalar va taxta yo'lka. Daryodan o'tish uchun."),
    ("eq_fortify",  "woeitm08", "Istehkom to'plami",       "", 1, (0.58, 0.50, 0.38),
     "Qopqoq, sim va tayanch. Pozitsiyani mudofaaga tayyorlaydi."),
    ("eq_infantry", "woeitm09", "Standart piyoda jihozi",  "", 1, (0.70, 0.66, 0.50),
     "Bo'linmaning bir kunlik to'liq jihozi. Zavod bir buyurtmada chiqaradi."),

    ("tool_engineer", "woeitm10", "Muhandis asboblari", "", 2, (0.75, 0.72, 0.66),
     "Shaxsiy asbob to'plami. Sarflanmaydi — bir marta yasaladi va qoladi."),
]

# --- recipes ----------------------------------------------------------------
# id, uid, name, inputs [(item_id, amount)], output, out amount, seconds, description
RECIPE_DEFS = [
    ("rec_infantry", "woercp01", "Standart piyoda jihozi",
     [("res_iron", 20), ("res_wood", 10), ("res_coal", 5)], "eq_infantry", 1, 4.0,
     "Zavodning asosiy buyurtmasi. Ishchilar bir smenada bo'linmani to'liq jihozlaydi."),
    ("rec_repair", "woercp02", "Ta'mirlash to'plami",
     [("res_iron", 8), ("res_wood", 6)], "eq_repair", 1, 3.0,
     "Ehtiyot qism va payvand to'plami — ishdan chiqqan uskuna uchun."),
    ("rec_install", "woercp03", "O'rnatish komplekti",
     [("res_iron", 10), ("res_coal", 4)], "eq_install", 1, 3.0,
     "Yangi qurilmani joyiga o'rnatish uchun kabel va mahkamlagichlar."),
    ("rec_bridge", "woercp04", "Ponton to'plami",
     [("res_wood", 14), ("res_iron", 8)], "eq_bridge", 1, 4.5,
     "Suzuvchi seksiyalar. Kolonna daryodan quruq o'tadi."),
    ("rec_fortify", "woercp05", "Istehkom to'plami",
     [("res_stone", 12), ("res_wood", 8)], "eq_fortify", 1, 3.5,
     "Pozitsiyani mudofaaga tayyorlaydigan qopqoq va tayanchlar."),
    ("rec_tools", "woercp06", "Muhandis asboblari",
     [("res_iron", 12), ("res_coal", 6)], "tool_engineer", 1, 3.0,
     "Shaxsiy asbob to'plami. Bir marta yasaladi, keyin doim yoningizda."),
]

# --- NPC roster (TZ 12: ishchi / muhandis / askar / ofitser / qishloq aholisi) ---
NPC_SCRIPT = "res://scripts/data/NpcData.cs"
NPCS = os.path.join(RES, "npcs")

# id, uid, name, role plate, behavior (0=Work 1=Patrol 2=Guard), tint, walk model, line
NPC_DEFS = [
    ("npc_worker", "woenpc01", "Ishchi", "ISHCHI", 0, (0.88, 0.80, 0.62), False,
     "Ish yurishmoqda, muhandis!"),
    ("npc_engineer", "woenpc02", "Muhandis", "MUHANDIS", 0, (0.68, 0.80, 0.92), False,
     "Chizma to'g'ri chiqdi — davom etamiz."),
    ("npc_soldier", "woenpc03", "Askar", "ASKAR", 2, (0.60, 0.68, 0.50), False,
     "Pozitsiya mustahkam. Rahmat."),
    ("npc_officer", "woenpc04", "Ofitser", "OFITSER", 0, (0.92, 0.82, 0.55), False,
     "Shtab hisobotni oldi. Yaxshi ish."),
    ("npc_villager", "woenpc05", "Qishloq aholisi", "AHOLI", 1, (0.94, 0.88, 0.82), True,
     "Rahmat! Endi yo'l ochildi."),
    ("npc_courier", "woenpc06", "Aloqachi", "ALOQACHI", 1, (0.80, 0.74, 0.90), True,
     "Xabarni shtabga yetkazaman."),
]

ITEM_UID = {d[0]: d[1] for d in ITEM_DEFS}
RECIPE_UID = {d[0]: d[1] for d in RECIPE_DEFS}
NPC_UID = {d[0]: d[1] for d in NPC_DEFS}


def npc_path(npc_id):
    return f"res://resources/data/npcs/{npc_id}.tres"


def item_path(item_id):
    return f"res://resources/data/items/item_{item_id}.tres"


def recipe_path(recipe_id):
    return f"res://resources/data/recipes/{recipe_id}.tres"


def write(path, text):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with open(path, "w", encoding="utf-8") as handle:
        handle.write(text)


# --- 1. items ---------------------------------------------------------------
def gen_items():
    for order, (item_id, uid, name, short, category, tint, desc) in enumerate(ITEM_DEFS):
        r, g, b = tint
        text = f'''[gd_resource type="Resource" script_class="ItemData" load_steps=2 format=3 uid="uid://{uid}"]

[ext_resource type="Script" path="{ITEM_SCRIPT}" id="1"]

[resource]
script = ExtResource("1")
ItemId = "{item_id}"
DisplayName = "{name}"
Description = "{desc}"
Category = {category}
ShortLabel = "{short}"
Tint = Color({r}, {g}, {b}, 1)
SortOrder = {order}
'''
        write(os.path.join(ITEMS, f"item_{item_id}.tres"), text)
    print(f"items: {len(ITEM_DEFS)}")


# --- 2. recipes -------------------------------------------------------------
def gen_recipes():
    for recipe_id, uid, name, inputs, output, out_amount, seconds, desc in RECIPE_DEFS:
        ext = [f'[ext_resource type="Script" path="{RECIPE_SCRIPT}" id="1"]']
        refs = []
        next_id = 2
        seen = {}
        for item_id, _ in inputs:
            if item_id not in seen:
                seen[item_id] = str(next_id)
                ext.append(
                    f'[ext_resource type="Resource" uid="uid://{ITEM_UID[item_id]}" '
                    f'path="{item_path(item_id)}" id="{next_id}"]')
                next_id += 1
            refs.append(seen[item_id])

        out_ref = seen.get(output)
        if out_ref is None:
            out_ref = str(next_id)
            ext.append(
                f'[ext_resource type="Resource" uid="uid://{ITEM_UID[output]}" '
                f'path="{item_path(output)}" id="{next_id}"]')
            next_id += 1

        inputs_array = ", ".join(f'ExtResource("{ref}")' for ref in refs)
        amounts = ", ".join(str(amount) for _, amount in inputs)

        text = f'''[gd_resource type="Resource" script_class="RecipeData" load_steps={next_id} format=3 uid="uid://{uid}"]

{chr(10).join(ext)}

[resource]
script = ExtResource("1")
RecipeId = "{recipe_id}"
DisplayName = "{name}"
Description = "{desc}"
Inputs = [{inputs_array}]
InputAmounts = PackedInt32Array({amounts})
Output = ExtResource("{out_ref}")
OutputAmount = {out_amount}
CraftSeconds = {seconds}
'''
        write(os.path.join(RECIPES, f"{recipe_id}.tres"), text)
    print(f"recipes: {len(RECIPE_DEFS)}")


# --- 2b. npcs ---------------------------------------------------------------
def gen_npcs():
    for npc_id, uid, name, role, behavior, tint, walks, line in NPC_DEFS:
        r, g, b = tint
        text = f'''[gd_resource type="Resource" script_class="NpcData" load_steps=2 format=3 uid="uid://{uid}"]

[ext_resource type="Script" path="{NPC_SCRIPT}" id="1"]

[resource]
script = ExtResource("1")
NpcId = "{npc_id}"
DisplayName = "{name}"
RoleLabel = "{role}"
Behavior = {behavior}
Tint = Color({r}, {g}, {b}, 1)
MoveSpeed = 1.7
PatrolRadius = 7.0
PauseSeconds = 2.5
ScanDegrees = 50.0
MissionLine = "{line}"
UsesWalkModel = {"true" if walks else "false"}
'''
        write(os.path.join(NPCS, f"{npc_id}.tres"), text)
    print(f"npcs: {len(NPC_DEFS)}")


# --- shared .tres patching helpers ------------------------------------------
MARK = "; --- production layer (generated) ---"


def strip_generated(text):
    """Removes a previous run's appended block and its ext_resources."""
    text = re.sub(r"\n" + re.escape(MARK) + r".*$", "\n", text, flags=re.S)
    text = re.sub(r'^\[ext_resource[^\n]*id="gen_[^"]*"\][^\n]*\n', "", text, flags=re.M)
    return text


def patch(path, ext_lines, property_lines):
    with open(path, encoding="utf-8") as handle:
        text = strip_generated(handle.read())

    if ext_lines:
        last_ext = None
        for match in re.finditer(r"^\[ext_resource[^\n]*\]\n", text, flags=re.M):
            last_ext = match
        insert_at = last_ext.end() if last_ext else text.index("\n[resource]") + 1
        text = text[:insert_at] + "\n".join(ext_lines) + "\n" + text[insert_at:]

    # load_steps must count every ext_resource plus the resource itself.
    steps = len(re.findall(r"^\[ext_resource", text, flags=re.M)) + 1
    text = re.sub(r"load_steps=\d+", f"load_steps={steps}", text, count=1)

    text = text.rstrip("\n") + "\n" + MARK + "\n" + "\n".join(property_lines) + "\n"
    write(path, text)


# --- 3. eras ----------------------------------------------------------------
ERA_YEARS = [1900, 1914, 1939, 1943, 1965, 1980, 1999, 2015, 2030, 2050]

# Equipment each work category calls for.
KIND_EQUIPMENT = {
    "TA'MIRLASH": "eq_repair",
    "O'RNATISH": "eq_install",
    "QURILISH": "eq_bridge",
    "ISTEHKOM": "eq_fortify",
    "XAVFSIZLIK": "eq_infantry",
}

# Who staffs a mission map: an engineer and worker on site, a guard, a courier.
MAP_CREW = ["npc_engineer", "npc_worker", "npc_soldier", "npc_courier"]

# Deposits placed on every map: item, and the floor for units per harvest.
# The actual yield is raised per era so the map always covers what its own gated
# tasks cost — otherwise a mission can generate that is impossible to finish.
DEPOSITS = [
    ("res_wood", 5), ("res_wood", 5),
    ("res_iron", 4), ("res_iron", 4),
    ("res_coal", 4),
    ("res_stone", 5),
]

# How much more than the bare requirement a map should hold, so the player can
# mis-order once and still finish.
RESOURCE_MARGIN = 1.6

# How many times one deposit can be harvested.
USES_PER_DEPOSIT = 3

RECIPE_BY_OUTPUT = {d[4]: d for d in RECIPE_DEFS}


def deposit_amounts(era_equipment, uses):
    """Units per harvest for each deposit, sized to the era's actual demand."""
    needed = {}
    for equipment in era_equipment:
        recipe = RECIPE_BY_OUTPUT.get(equipment)
        if recipe is None:
            continue
        for item_id, amount in recipe[3]:
            needed[item_id] = needed.get(item_id, 0) + amount

    counts = {}
    for item_id, _ in DEPOSITS:
        counts[item_id] = counts.get(item_id, 0) + 1

    amounts = []
    for item_id, floor in DEPOSITS:
        want = needed.get(item_id, 0) * RESOURCE_MARGIN
        per_deposit = math.ceil(want / (counts[item_id] * uses)) if want else 0
        amounts.append(max(floor, per_deposit))

    return amounts


def advance_days(index):
    """Days one task moves the calendar: about a quarter of the gap to the next era."""
    if index < len(ERA_YEARS) - 1:
        gap = ERA_YEARS[index + 1] - ERA_YEARS[index]
    else:
        gap = 16
    return round(gap / 4.0 * 365.0, 1)


def read_vec3_array(text, field):
    match = re.search(field + r" = PackedVector3Array\(([^)]*)\)", text)
    if not match or not match.group(1).strip():
        return []
    numbers = [float(n) for n in match.group(1).split(",")]
    return [(numbers[i], numbers[i + 1], numbers[i + 2]) for i in range(0, len(numbers), 3)]


def read_vec3(text, field):
    match = re.search(field + r" = Vector3\(([^)]*)\)", text)
    if not match:
        return (0.0, 0.0, 0.0)
    numbers = [float(n) for n in match.group(1).split(",")]
    return (numbers[0], numbers[1], numbers[2])


def place(occupied, rng, count, min_gap, radius_range, half):
    """Picks `count` spots that clear everything already on the map."""
    chosen = []
    attempts = 0
    while len(chosen) < count and attempts < 4000:
        attempts += 1
        angle = rng.uniform(0, math.tau)
        radius = rng.uniform(*radius_range)
        x = round(radius * math.cos(angle), 1)
        z = round(radius * math.sin(angle), 1)
        if abs(x) > half or abs(z) > half:
            continue
        if all((x - ox) ** 2 + (z - oz) ** 2 >= min_gap ** 2 for ox, _, oz in occupied + chosen):
            chosen.append((x, 0.0, z))
    return chosen


def gen_maps_and_tasks():
    for index in range(10):
        era = index + 1
        rng = random.Random(4200 + era)

        map_path = os.path.join(MAPS, f"mapdata_e{era}.tres")
        with open(map_path, encoding="utf-8") as handle:
            map_text = strip_generated(handle.read())

        ground = float(re.search(r"GroundSize = ([\d.]+)", map_text).group(1))
        half = ground / 2.0 - 6.0

        occupied = []
        for field in ("TaskPositions", "CratePositions", "GrovePositions", "EnemyPositions"):
            occupied += read_vec3_array(map_text, field)
        spawn = read_vec3(map_text, "SpawnPosition")
        occupied.append((spawn[0], 0.0, spawn[2]))

        # The workshop sits within sight of the arrival point; deposits ring the field.
        workshop = place(occupied, rng, 1, 6.0, (9.0, 15.0), half)[0]
        occupied.append(workshop)
        deposits = place(occupied, rng, len(DEPOSITS), 5.0, (12.0, min(28.0, half)), half)

        ext_lines = []
        item_ref = {}
        for slot, (item_id, _) in enumerate(DEPOSITS):
            if item_id not in item_ref:
                ref = f"gen_i{len(item_ref)}"
                item_ref[item_id] = ref
                ext_lines.append(
                    f'[ext_resource type="Resource" uid="uid://{ITEM_UID[item_id]}" '
                    f'path="{item_path(item_id)}" id="{ref}"]')

        recipe_refs = []
        for slot, definition in enumerate(RECIPE_DEFS):
            ref = f"gen_r{slot}"
            recipe_refs.append(ref)
            ext_lines.append(
                f'[ext_resource type="Resource" uid="uid://{definition[1]}" '
                f'path="{recipe_path(definition[0])}" id="{ref}"]')

        # Inhabitants: a crew at the workshop, a guard, and someone on the move.
        occupied += deposits
        crew = place(occupied, rng, len(MAP_CREW), 3.5, (7.0, 20.0), half)
        for slot, npc_id in enumerate(MAP_CREW):
            ext_lines.append(
                f'[ext_resource type="Resource" uid="uid://{NPC_UID[npc_id]}" '
                f'path="{npc_path(npc_id)}" id="gen_n{slot}"]')

        # Size the deposits against what this era's gated tasks actually cost.
        era_equipment = []
        for slot in (2, 3):
            task_path = os.path.join(TASKS, f"task_e{era}_{slot}.tres")
            with open(task_path, encoding="utf-8") as handle:
                kind_match = re.search(r'Kind = "([^"]*)"', strip_generated(handle.read()))
            era_equipment.append(KIND_EQUIPMENT.get(kind_match.group(1) if kind_match else "", "eq_repair"))

        node_amounts = deposit_amounts(era_equipment, USES_PER_DEPOSIT)

        nodes = ", ".join(f'ExtResource("{item_ref[item_id]}")' for item_id, _ in DEPOSITS)
        positions = ", ".join(f"{x}, {y}, {z}" for x, y, z in deposits)
        amounts = ", ".join(str(amount) for amount in node_amounts)
        recipes = ", ".join(f'ExtResource("{ref}")' for ref in recipe_refs)
        npcs = ", ".join(f'ExtResource("gen_n{slot}")' for slot in range(len(MAP_CREW)))
        npc_positions = ", ".join(f"{x}, {y}, {z}" for x, y, z in crew)
        npc_yaws = ", ".join(str(round(rng.uniform(-180, 180), 1)) for _ in MAP_CREW)

        patch(map_path, ext_lines, [
            f"ResourceNodes = [{nodes}]",
            f"ResourceNodePositions = PackedVector3Array({positions})",
            f"ResourceNodeAmounts = PackedInt32Array({amounts})",
            f"ResourceNodeUses = {USES_PER_DEPOSIT}",
            "HasWorkshop = true",
            f"WorkshopPosition = Vector3({workshop[0]}, 0, {workshop[2]})",
            f"WorkshopRotationY = {round(rng.uniform(-40, 40), 1)}",
            f"Recipes = [{recipes}]",
            f"Npcs = [{npcs}]",
            f"NpcPositions = PackedVector3Array({npc_positions})",
            f"NpcRotationsY = PackedFloat32Array({npc_yaws})",
        ])

        # --- tasks: gate the 2nd and 3rd of each mission, and move the calendar ---
        days = advance_days(index)
        for slot in (1, 2, 3):
            task_path = os.path.join(TASKS, f"task_e{era}_{slot}.tres")
            with open(task_path, encoding="utf-8") as handle:
                task_text = strip_generated(handle.read())

            kind_match = re.search(r'Kind = "([^"]*)"', task_text)
            kind = kind_match.group(1) if kind_match else ""
            equipment = KIND_EQUIPMENT.get(kind, "eq_repair")

            # The first task of a mission stays open so arrival is never a dead end.
            if slot == 1:
                patch(task_path, [], [f"AdvanceDays = {days}"])
                continue

            patch(task_path, [
                f'[ext_resource type="Resource" uid="uid://{ITEM_UID[equipment]}" '
                f'path="{item_path(equipment)}" id="gen_eq"]',
            ], [
                'RequiredItem = ExtResource("gen_eq")',
                "RequiredAmount = 1",
                "ConsumesRequirement = true",
                f"AdvanceDays = {days}",
            ])

        # --- mission: era year, and the campaign's closing mission ---
        mission_path = os.path.join(MISSIONS, f"mission_e{era}.tres")
        mission_lines = [f"EraYear = {ERA_YEARS[index]}"]
        if era == 10:
            mission_lines.append("IsFinal = true")
        patch(mission_path, [], mission_lines)

    print("maps/tasks/missions: 10 eras patched")


def verify_maps_are_solvable():
    """
    Re-checks what was just written: every gated task's equipment must be
    producible from that map's own deposits. A generated mission that cannot be
    finished only shows up several minutes into a playtest, so it is worth
    failing loudly here instead.
    """
    problems = 0
    for index in range(10):
        era = index + 1
        map_path = os.path.join(MAPS, f"mapdata_e{era}.tres")
        with open(map_path, encoding="utf-8") as handle:
            map_text = handle.read()

        amounts = [int(n) for n in re.search(
            r"ResourceNodeAmounts = PackedInt32Array\(([^)]*)\)", map_text).group(1).split(",")]
        uses = int(re.search(r"ResourceNodeUses = (\d+)", map_text).group(1))

        available = {}
        for (item_id, _), amount in zip(DEPOSITS, amounts):
            available[item_id] = available.get(item_id, 0) + amount * uses

        needed = {}
        for slot in (2, 3):
            task_path = os.path.join(TASKS, f"task_e{era}_{slot}.tres")
            with open(task_path, encoding="utf-8") as handle:
                kind = re.search(r'Kind = "([^"]*)"', handle.read())
            equipment = KIND_EQUIPMENT.get(kind.group(1) if kind else "", "eq_repair")
            recipe = RECIPE_BY_OUTPUT.get(equipment)
            if recipe is None:
                print(f"  !! e{era}: nothing produces {equipment}")
                problems += 1
                continue
            for item_id, amount in recipe[3]:
                needed[item_id] = needed.get(item_id, 0) + amount

        for item_id, want in needed.items():
            have = available.get(item_id, 0)
            if have < want:
                print(f"  !! e{era}: {item_id} short — map yields {have}, tasks need {want}")
                problems += 1

    print("verify: all 10 maps solvable" if problems == 0 else f"verify: {problems} PROBLEM(S)")
    return problems == 0


if __name__ == "__main__":
    gen_items()
    gen_recipes()
    gen_npcs()
    gen_maps_and_tasks()
    verify_maps_are_solvable()
