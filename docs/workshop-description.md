# 工坊描述文案（Repair Bench）

> 当前发布：**1.0.0**（已打包：`release\RepairBench\RepairBench-1.0.0\` 与 `RepairBench-1.0.0.zip`；**已首次上传创意工坊，PublishedFileId = 3837851806**，2026-11-11）｜目标：RimWorld 1.7 ｜依赖：无
> 更新日期：2026-11-11（文案改版：只写玩家能感知的内容，不写实现细节）
>
> 受众是**玩家**，不是开发者：只写他们能看到的玩法、花费和问题；不写引擎/动画/音效/程序集/XML 结构这类内部实现，也不解释 mod 之间"谁让位给谁"的机制。
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
给 RimWorld 1.7 加一条修装备的路：研究「修补技术」→ 造修补台 → 用 10 钢铁 + 5 任意织物/皮革做通用修补剂 → 标记地上的磨损装备，小人搬到台面修回 100% 耐久，品质、材质、词条全保留。三种发起方式（命令标记 / 物品命令栏 / 右键强制维修），断点可续修，缺台缺料有警报。只修武器/衣物/护甲。
```

## 工坊描述（中文，直接整段复制）

```bbcode
[b]Repair Bench —— 修补台：把磨坏的武器、衣物、护甲修回满耐久[/b]

原版只能修建筑，装备磨损了只能换新的。这个 mod 给你另一条路：把坏掉的装备标记出来，小人搬到修补台，消耗通用「修补剂」修回 100% 耐久——品质、材质、词条都保留。修补剂不分钢、布、皮，什么装备都能修。

[h2]怎么用[/h2]
- 研究「修补技术」
- 造修补台：任意金属或木材 75 + 15 钢铁
- 做修补剂：10 钢铁 + 5 任意织物或皮革 → 5 个，装进台里
- 选一种方式发起：
  - Orders 菜单的「修补」：单点一件，或按住左键拖一个框
  - 选中装备 → 命令栏的「维修」
  - 右键装备 →「维修」或「强制维修」
- 小人把装备摆到台面上修满，修完留在台旁

[h2]内容[/h2]
[b]◆ 修补台[/b]
- 3×1，可搬动、可涂漆，颜色跟着建造材料走
- 能存 20 个修补剂

[b]◆ 修补剂[/b]
- 通用材料，不看装备材质，钢武器、布衣服、皮革护甲都用同一种

[b]◆ 三种发起方式[/b]
- 命令标记：单件或框选，同原版标记手感
- 物品命令栏：选中磨损装备就有按钮，不用先点标记
- 右键：普通「维修」只打标记；「强制维修」立刻把选中的小人派过去；右键修补台可以逐件派活

[b]◆ 中断与进度[/b]
- 被打断、换班、读档都不会白干，回来接着修
- 修满就是 100%，品质、材质、词条不变

[b]◆ 提示[/b]
- 有装备等着修却没台或缺料时，右侧会出现醒目警报
- 台子缺料会亮图标，选中台子能看到余量

[h2]兼容性[/h2]
- 游戏版本：RimWorld 1.7
- 无需前置，全 DLC 可用
- 纯新增内容，不替换原版内容
- 与其它维修 mod 功能重叠，可以共存，但没必要同时装
- 旧存档可以直接加

[h2]常见问题[/h2]
[b]Q：为什么修不了穿在身上/拿在手里的装备？[/b]
A：只修地上的装备。先用原版的方式把它丢到地上，再标记。

[b]Q：能修建筑吗？[/b]
A：不能，建筑交给原版「修理」。

[b]Q：修完的东西在哪？[/b]
A：在修补台旁边，需要自己搬回去。

[b]Q：标了没反应？[/b]
A：按顺序看：右侧警报是不是缺台或缺料 → 台里有没有修补剂 → 小人的「修补」工作有没有启用 → 装备是不是在地上。

[b]Q：品质会掉吗？[/b]
A：不会，耐久直接修满。

[b]Q：会不会太强？[/b]
A：一件装备从 0 修满大约要 20 钢铁 + 10 织物或皮革，外加研究和工时。嫌贵或嫌便宜都能自己改。

[hr]
[h2]反馈[/h2]
bug、平衡意见、想要的功能都欢迎在下面留言。
```

## Description (English, paste into the English description box)

```bbcode
[b]Repair Bench — repair worn weapons, apparel and armor back to full hit points[/b]

Vanilla only repairs buildings, so worn gear has to be replaced. This mod adds another way: mark the damaged gear, a colonist hauls it to a repair bench and spends generic repair kits to restore it to 100% hit points — quality, material and traits are kept. Kits don't care about steel, cloth or leather, so gear from other mods can be repaired too.

[h2]How to use[/h2]
- Research "repair techniques"
- Build a repair bench: 75 of any metal or wood + 15 steel
- Craft repair kits: 10 steel + 5 of any textile or leather yields 5 kits; load them into the bench
- Start a repair in any of three ways:
  - The "repair" order in the Orders menu: click one item or drag a box
  - Select a damaged item and use the "Repair" button in its command bar
  - Right-click the item for "Repair" or "Force repair"
- A colonist places the item on the bench top and repairs it to full; the item is left next to the bench

[h2]What you get[/h2]
[b]◆ The bench[/b]
- 3x1, minifiable and paintable, tinted by its building material
- Holds 20 repair kits

[b]◆ Repair kits[/b]
- One generic material: steel weapons, cloth clothes and leather armor all use the same kit

[b]◆ Three ways to start[/b]
- Order marking: single item or a dragged box, same feel as vanilla orders
- Item command bar: a button right on the damaged item
- Right-click: "Repair" just marks it; "Force repair" sends the selected colonist immediately; right-clicking the bench lets you dispatch pending items one by one

[b]◆ Interruptions[/b]
- Getting interrupted, changing shifts or reloading a save doesn't waste the work; it resumes where it stopped
- A finished repair is exactly 100%; quality, material and traits are untouched

[b]◆ Warnings[/b]
- A prominent alert when gear is waiting for repair but there is no usable bench or not enough kits
- A missing-kit icon on the bench and a kit gauge when it is selected

[h2]Compatibility[/h2]
- Game version: RimWorld 1.7
- No requirements, works with all DLC
- Pure addition, nothing vanilla is replaced
- Overlaps with other repair mods; they can coexist but you don't need both
- Existing saves are fine

[h2]FAQ[/h2]
[b]Q: Why can't I repair gear that is worn or equipped?[/b]
A: Only gear on the ground can be repaired. Drop it first, then mark it.

[b]Q: Can it repair buildings?[/b]
A: No, buildings use the vanilla repair work.

[b]Q: Where does the repaired item go?[/b]
A: It stays next to the bench; haul it back yourself.

[b]Q: I marked something and nothing happens.[/b]
A: Check the alert (missing bench or kits), whether the bench has kits, whether the colonist's "repair" work is enabled, and whether the item is on the ground.

[b]Q: Does quality drop?[/b]
A: No, hit points go straight to full.

[b]Q: Is it overpowered?[/b]
A: One full repair takes roughly 20 steel + 10 textile/leather, plus research and work time; the numbers are easy to change in the XML if you want different balance.

[hr]
[h2]Feedback[/h2]
Bugs, balance notes and feature requests are welcome below.
```

## 其它可填项

- **标签**：Gameplay、Items、Weapons（以上传对话框里实际提供的为准；不选 Translation，本 mod 自带中英双语）
- **预览图**：`About/Preview.png`（1280×720，自制封面：REPAIR BENCH + 修补台 + 贴图道具）
- **可见性**：公开
- **更改说明（游戏自动生成）**：游戏内上传会附一条 `[Auto-generated text]: Update on <时间>.`，不可自定义；首版没有这条

## 内部记录（不贴 Steam）

- 2026-11-11：首次上传创意工坊，PublishedFileId `3837851806`（从此以后游戏内更新走「更新已有条目」）。
- v1.0.0（2026-11-11）：首个发布版——修补台（3×1、按材料染色、可涂漆、20 个修补剂容量）、通用修补剂（10 钢铁 + 5 任意织物/皮革 → 5）、修补技术研究（600、前置 Smithing）、Orders 修补标记（可拖框）、物品命令栏维修按钮、右键维修 / 强制维修（装备与台两种入口）、按缺失耐久扣料与计时（满耐久 3000 ticks、10 个修补剂）、断点续修 + 读档保留进度（MapComponent 存档）、四层缺料提示（High 警报 / 台面脉动图标 / 燃料 gizmo / 检视红字）、自制金属屑粒子与锉磨/扣合音、自制工作动画（检测到 Yayo's Animation 时让位给它）、中英双语。

## 维护提示

1. 文案面向玩家：写"玩家看到什么、要花什么、怎么排错"，不写实现细节（引擎、动画、音效、Harmony、XML 结构、MapComponent 存档方式等）。技术细节放本文件「内部记录」与 README。
2. 改功能时同步：本文档「内容」段 → `About/About.xml` 的 `<description>`（纯文本，不能用 BBCode）→ 工坊页面（手动改）。
3. 发版流程：三处版本号同步（`About/About.xml` / `RepairBenchMod.cs` / `.csproj`）→ build → `tools\verify.ps1 -Strict` → `tools\deploy.ps1` → `tools\make-release.ps1` → `git tag v<版本>` → 游戏内上传/更新。
4. 工坊条目 ID = **3837851806**（首次上传 2026-11-11 生成，存在 `About\PublishedFileId.txt`；不进 git，部署脚本会先备份再写回）。**这个文件不能丢**，丢了再上传会新建重复工坊条目。
5. 粘贴后自检：无 Markdown（`#`、`**`）、无 HTML、BBCode 标签成对闭合、正文没有「安装」「更新日志」板块。
