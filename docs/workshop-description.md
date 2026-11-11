# 工坊描述文案（Repair Bench）

> 当前发布：**1.0.0**（已打包：`release\RepairBench\RepairBench-1.0.0\` 与 `RepairBench-1.0.0.zip`，392,037 B；无 PublishedFileId.txt，属首次上传）｜目标：RimWorld 1.7 ｜依赖：无
> 更新日期：2026-11-11（1.0.0：首个发布版）
>
> 格式：**Steam BBCode**（`[b]`、`[h2]`、`[hr]`、`[url=]`），不是 Markdown / HTML；行首 `- ` 只是短横线当子弹点用。
> 结构约定（沿用维护者 2026-11-08 的要求）：**正文不放「安装」「更新日志」两个板块**；更新日志只在本文件末尾「内部记录」里维护。
> 两个「描述」不是一回事：`About/About.xml` 的 `<description>` 是游戏内纯文本，这里是工坊页 BBCode，两边内容保持一致。
> 工坊页描述**只能去 Steam 网页/客户端手动改**——游戏内「上传/更新」不会更新已有条目的描述。

## 标题

首选：

```text
Repair Bench —— 修补台：把磨坏的武器、衣物、护甲修回满耐久 | 1.7
```

备选：

```text
Repair Bench 修补台 | 1.7
```

```text
Repair Bench —— 让磨损的装备能修回 100% | 1.7
```

## 摘要 / 列表短描述

```text
给 RimWorld 1.7 加一条修装备的路：研究「修补技术」→ 造修补台 → 用 10 钢铁 + 5 任意织物/皮革做通用修补剂 → 标记地上的磨损装备（命令标记 / 物品命令栏 / 右键强制维修三种方式），小人搬到台面修回 100% 耐久，品质、材质、词条全保留。断点可续修、读档保留进度；缺台缺料有警报和台面图标提示。只修武器/衣物/护甲（不修建筑），纯新增内容。
```

## 工坊描述（中文，直接整段复制）

```bbcode
[b]Repair Bench —— 修补台：把磨坏的武器、衣物、护甲修回满耐久[/b]

原版只有「修建筑」的修理工作，装备磨损了只能换新的。这个 mod 补上另一条路：把坏掉的装备标记出来，小人搬到修补台，消耗一种通用「修补剂」把它修回 100% 耐久——品质、材质、词条、耐久上限全部原样保留。

修补剂不分钢、布、皮，所以任何 mod 的武器/衣物/护甲都能修。

[h2]怎么用[/h2]
- 研究「修补技术」（600 点，前置 Smithing）
- 造修补台：任意金属或木材 75 + 15 钢铁
- 做修补剂：10 钢铁 + 5 任意织物或皮革 → 5 个，装进台里（选中台 → 装料）
- 发起修理，三种方式任选：
  - Orders 菜单的「修补」标记：点选单件，或按住左键拖一个框（同原版标记手感）
  - 选中装备 → 命令栏出现「维修装备」按钮（已标记时变成「取消维修」）
  - 右键装备 →「维修装备」；右键修补台还能逐件「强制维修」
- 小人把装备摆到台面上，消耗修补剂修满，修完留在台旁

[h2]功能[/h2]
[b]◆ 三种发起方式[/b]
- 命令标记：框选/拖拽，一次标一片，和原版「搬运」标记一个手感
- 物品命令栏：选中磨损装备就直接有按钮，不用先点标记
- 右键菜单：普通的「维修装备」只打标记；「强制维修」立刻把选中的小人派过去（不用等工作优先级轮到它），右键修补台则列出所有待修物品，逐件指派

[b]◆ 修补台[/b]
- 3×1，可搬动、可涂漆；台体按建造材料染色（钢铁灰 / 木材棕 / 塑钢淡青）
- 台里能存 20 个修补剂，缺料有专门提示
- 工作点显示原版工作台那种座椅图形，物品会真的摆到台面上修

[b]◆ 修补剂与消耗[/b]
- 通用材料，不看装备材质：钢武器、布衣服、皮革护甲都用同一种
- 10 钢铁 + 5 任意织物/皮革 做 5 个；一件装备从 0 修到满大约要 10 个（≈ 2 批材料）
- 修理时间按缺失耐久比例算，满耐久基准约 50 秒，受「通用工作速度」和手工技能影响

[b]◆ 中断与进度[/b]
- 中途被打断、换班、读档都不白干：进度按物品记录，回来接着修
- 修满就是 100%，不改品质、材质、词条、耐久上限，不换件、不加属性

[b]◆ 提示（不用翻面板）[/b]
- High 级警报：有装备在等修补，但地图上没台或缺料
- 台子上的脉动「缺料」图标
- 选中台时的修补剂余量条
- 检视面板红字「修补剂不足」

[b]◆ 手感[/b]
- 自制金属屑粒子 + 锉磨循环音 + 修好扣合音，不引用原版素材
- 工作时小人有前后锉磨的动作；装了 Yayo's Animation 就自动交给它播，不会两边抢动画

[h2]兼容性[/h2]
- 游戏版本：RimWorld 1.7
- 前置：无。不加载 Harmony，只在检测到 Yayo's Animation 时借用它的 Harmony 实例做一次挂接
- DLC：全部兼容（只用 Core，不依赖任何 DLC）
- 纯新增内容：不改原版代码、不替换原版 Def，与常见 UI / 内容 mod 不冲突
- 与其它维修 mod（NanoRepairTech、MendAndRecycle、Repairable Gear 等）功能重叠，可以共存，但没必要同时装
- 旧存档可加：加材料系统之前造的台子，读档时会自动补成钢铁台（日志里会有一条提示）

[h2]常见问题[/h2]
[b]Q：为什么修不了穿在身上/拿在手里的装备？[/b]
A：v1 只修地上的装备。原版没有「脱下衣物」命令，请先把装备丢到地上再标记。

[b]Q：能修建筑吗？[/b]
A：不能。建筑交给原版「修理」工作，这个 mod 只处理武器 / 衣物 / 护甲。

[b]Q：修完的东西去哪了？[/b]
A：留在修补台旁边，需要你自己搬回仓库（v1 不自动归仓）。

[b]Q：标了没反应？[/b]
A：按顺序看：右侧警报是不是缺台/缺料 → 台里有没有修补剂 → 小人的「修补」工作类型有没有启用 → 装备是不是在地上、有没有被别人占用。

[b]Q：品质会掉吗？[/b]
A：不会。耐久直接修满，品质 / 材质 / 词条 / 耐久上限都不变。

[b]Q：会不会太强？[/b]
A：要材料也要工时：一件装备从 0 修满大约 20 钢铁 + 10 织物/皮革，外加 600 点研究和台子本身。嫌贵或嫌便宜都能自己改：修补剂配方、每个修补剂能修多少、工时都写在 Defs 的 XML 里。

[hr]
[h2]反馈[/h2]
bug、平衡意见、想要的功能（比如「穿戴中也能修」）都欢迎在下面留言；附上 Player.log 里的报错行更容易定位。
```

## Description (English, paste into the English description box)

```bbcode
[b]Repair Bench — repair worn weapons, apparel and armor back to full hit points[/b]

Vanilla only has a repair job for buildings; worn gear can only be replaced. This mod adds another way: mark the damaged gear, a colonist hauls it to a repair bench and spends generic "repair kits" to restore it to 100% hit points — quality, material, traits and hit point cap all stay exactly as they were.

Repair kits don't care about steel, cloth or leather, so gear from any other mod can be repaired too.

[h2]How to use[/h2]
- Research "repair techniques" (600 points, requires Smithing)
- Build a repair bench: 75 of any metal or wood + 15 steel
- Craft repair kits: 10 steel + 5 of any textile or leather yields 5 kits; load them into the bench (select it, then "Load")
- Start a repair in any of three ways:
  - The "repair" order in the Orders menu: click a single item or drag a box (same feel as vanilla orders)
  - Select a damaged item: a "Repair item" button appears in the command bar (it becomes "Cancel repair" once marked)
  - Right-click the item: "Repair item"; right-click the bench for per-item "Force repair"
- A colonist places the item on the bench top, spends kits and repairs it to full; the item is left next to the bench

[h2]Features[/h2]
[b]◆ Three ways to start[/b]
- Order marking: click or drag a box, mark a whole pile at once, same feel as the vanilla haul order
- Item command bar: a button right on the selected item, no need to open the orders menu
- Right-click menu: plain "Repair item" just marks it; "Force repair" sends the selected colonist immediately (ignoring work priorities); right-clicking the bench lists every pending item so you can dispatch them one by one

[b]◆ The bench[/b]
- 3x1, minifiable and paintable; tinted by its building material (steel grey / wood brown / plasteel pale cyan)
- Holds 20 repair kits; a dedicated warning appears when it runs dry
- Shows the vanilla work-table interaction spot (chair) and the item is actually placed on the bench top while being worked on

[b]◆ Kits and cost[/b]
- One generic material regardless of gear material: steel weapons, cloth clothes and leather armor all use the same kit
- 10 steel + 5 of any textile/leather makes 5 kits; repairing one item from 0 to full takes about 10 kits (~2 batches)
- Work time scales with missing hit points, about 50 seconds for a full repair at base speed, modified by General Labor Speed and Crafting skill

[b]◆ Interruptions and progress[/b]
- Getting interrupted, changing shifts or reloading a save doesn't waste the work: progress is stored per item and resumes where it stopped
- A finished repair is exactly 100%; quality, material, traits and hit point cap are untouched

[b]◆ Feedback you can't miss[/b]
- A High priority alert when gear is waiting for repair but there is no usable bench or not enough kits
- A pulsing "out of fuel" icon on the bench
- A kit gauge on the selected bench
- A red "not enough repair kits" line in the inspect pane

[b]◆ Feel[/b]
- Custom metal-shaving particles, a filing loop and a finished "clunk" sound — no vanilla assets are reused
- The colonist plays a filing animation while working; if Yayo's Animation is installed the animation is handed over to it so the two never fight

[h2]Compatibility[/h2]
- Game version: RimWorld 1.7
- Requirements: none. Harmony is not loaded; only if Yayo's Animation is detected does the mod borrow its Harmony instance for a one-time hook
- DLC: all (Core only, no DLC dependency)
- Pure addition: no vanilla code is patched, no vanilla defs are replaced
- Coexists with Yayo's Animation (its work animation is used when present)
- Overlaps with other repair mods (NanoRepairTech, MendAndRecycle, Repairable Gear, ...); they can coexist but you don't need both
- Existing saves: benches built before the material system was added are migrated to steel on load (one log line explains it)

[h2]FAQ[/h2]
[b]Q: Why can't I repair gear that is worn or equipped?[/b]
A: v1 only repairs gear lying on the ground. Vanilla has no "take off apparel" order, so drop the item first and then mark it.

[b]Q: Can it repair buildings?[/b]
A: No. Buildings use the vanilla repair work; this mod only handles weapons, apparel and armor.

[b]Q: Where does the repaired item go?[/b]
A: It stays next to the bench; you haul it back yourself (v1 does not auto-haul).

[b]Q: I marked something and nothing happens.[/b]
A: Check in this order: is the alert about a missing bench or missing kits? Are there kits in the bench? Is the "repair" work type enabled for that colonist? Is the item on the ground and not reserved by someone else?

[b]Q: Does quality drop?[/b]
A: No. Hit points go straight to full; quality, material, traits and the hit point cap are unchanged.

[b]Q: Is it overpowered?[/b]
A: It costs materials and work: one full repair is roughly 20 steel + 10 textile/leather on top of 600 research points and the bench itself. Tuning is plain XML in Defs (kit recipe, kits per repair, work amount), so you can change it freely.

[hr]
[h2]Feedback[/h2]
Bugs, balance notes and feature requests (such as "repair worn gear") are welcome below; including the relevant Player.log error lines makes them much easier to track down.
```

## 其它可填项

- **标签**：Gameplay、Items、Weapons（以上传对话框里实际提供的为准；不选 Translation，本 mod 自带中英双语）
- **预览图**：`About/Preview.png`（1280×720，自制封面：REPAIR BENCH + 修补台 + 贴图道具）
- **可见性**：公开
- **更改说明（游戏自动生成）**：游戏内上传会附一条 `[Auto-generated text]: Update on <时间>.`，不可自定义；首版没有这条

## 内部记录（不贴 Steam）

- v1.0.0（2026-11-11）：首个发布版——修补台（3×1、按材料染色、可涂漆、20 个修补剂容量）、通用修补剂（10 钢铁 + 5 任意织物/皮革 → 5）、修补技术研究（600、前置 Smithing）、Orders 修补标记（可拖框）、物品命令栏维修按钮、右键维修 / 强制维修（装备与台两种入口）、按缺失耐久扣料与计时、断点续修 + 读档保留进度、四层缺料提示、自制金属屑粒子与锉磨/扣合音、自制工作动画（自动让位给 Yayo's Animation）、中英双语。

## 维护提示

1. 改功能时同步：本文档「功能」段 → `About/About.xml` 的 `<description>`（纯文本，不能用 BBCode）→ 工坊页面（手动改）。
2. 发版流程：三处版本号同步（`About/About.xml` / `RepairBenchMod.cs` / `.csproj`）→ build → `tools\verify.ps1 -Strict` → `tools\deploy.ps1` → `tools\make-release.ps1` → `git tag v<版本>` → 游戏内上传/更新。
3. 首次上传后游戏会写 `About\PublishedFileId.txt`，**不要丢**（不进 git、不进发布包）；之后每次部署脚本会先备份再写回，避免新建重复工坊条目。
4. 粘贴后自检：无 Markdown（`#`、`**`）、无 HTML、BBCode 标签成对闭合、正文没有「安装」「更新日志」板块。
