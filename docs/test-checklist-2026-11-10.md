# Repair Bench — 维护者测试清单（2026-11-10）

环境：RimWorld 1.7，只启用 Repair Bench（可选加 Harmony 依赖框架 mod）。
进入游戏前：本 mod 已部署到 `RimWorld/Mods/RepairBench`。

| # | 操作 | 预期 | 失败要什么 |
|---|------|------|-----------|
| 1 | 新开档，打开「研究」看 Medieval 页；先不研究「修补技术」，看「命令」菜单 | 未研究时命令菜单**没有**「修补」按钮；研究完成后按钮出现 | 截图（命令菜单 + 研究页）；Player.log 里搜 `RepairBench` |
| 2 | 研究「修补技术」→ 造修补台（钢材 75 + 布料 15）→ 在锻造台做修补剂 → 把修补剂搬到修补台上 | 修补台信息栏出现燃料条，显示 repair kits 满 20；可点「取出燃料」 | 截图（修补台 inspect 面板）；Player.log 搜 `CompRefuelable` / `RepairBench` |
| 3 | 给一件武器/衣物造成约 50% 损伤后丢到地上 → 点「修补」→ 点击该物品 | 物品出现修补标记；小人搬上台并**把装备摆在台面上**，出现进度条 + **自制金属屑飞溅** + 自制锉磨音（不再有原版焊花）；消耗约 5 个修补剂、修满 100%、标记消失、物品留在台旁地上 | 截图（标记 + 进度条 + 修完的耐久）；Player.log 搜 `Exception` |
| 4 | 让小人**穿着**破损护甲/衣物，用「修补」去点它 | 无法标记，浮出提示「请先把装备放到地上」；物品不挂标记 | 截图（提示气泡） |
| 5 | 有物品带修补标记，但**拆掉/未造**修补台，或把台上修补剂取空 | 右侧**靠上位置**出现 High 优先级 Alert「有装备等待修补」（出现时响一声提示音），文案为已翻译文本；点击 Alert 跳到相关物品/地图 | 截图（Alert 展开 + 点击后画面）；Player.log 搜 `Alert` |
| 6 | 修到一半时征召小人 / 让小人去做别的事打断，等它离开后再解除 | 回来后从**断点**继续（进度条不从 0 开始），最终能修满 | 截图（两次进度条对比）；Player.log |
| 7 | 修理进行中存档 → 退出到主菜单 → 读档 | 标记仍在、进度保留、修补台上修补剂数量与存档前一致 | Player.log 搜 `Scribe` / `RepairBenchProgress` |
| 8 | 切换语言到「简体中文」，看修补台 / 修补剂 / 研究 / 工作类型 / 设计器提示 / Alert / 右键菜单 | 全部是中文，没有 `RepairBench_...` 或 `repair kits` 之类的未翻译原文 | 截图（各面板）；missing translation 会在 Player.log 里以 `No translation` 出现 |
| 9 | **右键维修命令**：选中一个小人 → 右键点地上约 50% 耐久的武器 → 选「维修装备」 | 菜单项显示「维修装备」，悬停提示「需要约 5 个修补剂」；点击后物品挂上修补标记（和「修补」命令一样的效果）。对满耐久装备 / 已标记装备右键，菜单项变灰并说明原因 | 截图（右键菜单 + 悬停 tooltip）；Player.log 搜 `FloatMenuOptionProvider` |
| 10 | **修补剂图标**：造/拿到修补剂，分别看 1 个、约 10 个、50 个堆叠时的图标 | 不管堆多少个，都是**同一张**锈棕色工具箱图标（不再显示粉黑方块，也不再随数量变多箱子）；Player.log **不再**出现 `Collection cannot init: No textures found at path Things/Item/Resource/RepairBench/RepairKit` | 截图（三种堆叠）；Player.log 搜 `Collection cannot init` |
| 11 | **拖拽标记**：点「命令」→「修补」，在几件磨损装备上按住左键拖出一个矩形再松手 | 拖拽过程中有框选预览，松手后框内每件装备都挂上修补标记（和原版「搬运」标记手感一致）；用「取消」拖框能批量取消 | 截图（拖拽中的框 + 松手后的标记）；Player.log 搜 `Exception` |
| 12 | **研究树位置**：打开研究页 Medieval 区块 | 「修补技术」位于「锻造」正下方那一格，不与其它项目重叠、连线不交叉 | 截图（研究页局部） |
| 13 | **工作点样式**：造修补台，进入放置预览 | 工作格显示原版工作台同款的**座椅/椅子**图形（不再是白色圆圈） | 截图（放置预览） |
| 14 | **贴图观感与尺寸**：造好后看修补台本体（四个朝向都转一遍），对比旁边原版工作台 | 白台面 + 粗黑描边 + 台钳/锤子/工具箱的扁平象形道具，观感与原版工作台同一画风；台面大小与 3×1 占地对齐（左右各留一点透明边，不铺满相邻格）；东西朝向是竖版贴图 | 截图（南/北/东/西四向 + 旁边原版工作台同框） |
| 15 | **工作动画与音效**：让小人修一件 50% 耐久的装备，盯着修补台看几秒 | 装备**摆在修补台台面上**；小人面向台面；有**金属屑**从台面崩出并弹落（不是原版焊花/火花）+ 自制锉磨**循环**音；修好的瞬间有一声金属扣合；进度条画在物品上 | 录屏 5 秒或连续截图；Player.log 搜 `Exception` |
| 16 | **台面复用**：连着修两件装备 | 第一件修完被放回**台旁地上**、台面空出；第二件能正常摆上台面（不会因为台面被占而掉到地上） | 截图（修完第一件后的台面 + 第二件在台上） |
| 17 | **缺料的三层提示**：把修补台上的修补剂全部取出（或让它为 0）→ 选中修补台 → 再看有标记物品时的右侧警报 | ① 检视面板出现红字「修补剂不足，请先装入更多修补剂再修理。」；② 选中台就有修补剂余量条 Gizmo；③ 右侧靠上出现 High Alert「有装备等待修补」并响提示音 | 截图（检视面板红字 + 燃料条 + Alert）；Player.log 搜 `RepairBench_NoKitsInBench` |
| 18 | **红字消失确认**：进有修补台的存档，看 Player.log | 不再出现 `Config error in RepairBench_Bench: graphicData defines a shadowInfo but staticSunShadowHeight > 0` | 把 Player.log 里含 `RepairBench` 的行整段贴回来 |
| 19 | **缺料图标闪烁**：把修补台上的修补剂全部取出，**不要选中台子**，隔着屏幕看台子本体（可拉远视角） | 台子上浮起**脉动的缺料图标**（修补剂图标 + 红色禁止斜杠），一闪一闪，和原版燃料锻造台没燃料时完全一样；装上修补剂后图标消失 | 截图（拉近的台子，能看清图标）；Player.log 搜 `RepairBench` |
| 20 | **小人工作动画**：让小人修一件 50% 耐久的装备，盯住小人几秒 | 小人面向台面，身体做**明显的前后锉磨晃动**（约 ±7.5°，看得出在用劲，但不夸张到像抽搐）；打断/修完立刻停下恢复正常站姿，不会一直抖 | 录屏 5 秒；若觉得晃动太强/太弱，说方向（不是 bug，改 `Defs/AnimationDefs/RepairBench_Animation.xml` 里的 angle） |
| 21 | **装备命令栏「维修」按钮**：点选地上约 50% 耐久的武器/衣物/护甲（不是点小人） | 检查器底部命令栏出现「维修」按钮（扳手图标），点一下即挂修补标记（响一声、地图上出现标记）；再点同一件装备，同一位置变成「取消维修」，点击可撤掉标记。满耐久装备、穿在身上的装备、未研究「修补技术」时不出现该按钮。原版武器/护甲/衣物、以及从原版 `BaseWeapon` / `ApparelNoQualityBase` 派生的 mod 装备都应有此按钮 | 截图（命令栏 + 点击后的标记）；Player.log 搜 `CompRepairMarkable` |
| 22 | **右键强制维修（装备）**：选中一个小人 → 右键点地上约 50% 耐久的武器 → 选「强制维修」 | 小人**立刻**（不等工作队列扫描）去搬这件装备、上修补台修满；菜单项悬停显示需要几个修补剂。右键同一件已标记的装备，额外有「取消维修」一项；物品被别人占用 / 没有可用修补台时「强制维修」变灰并说明原因。**不应**再出现第二条原版风格的「优先执行维修」英文式选项 | 截图（右键菜单 + tooltip + 小人出发）；Player.log 搜 `Exception` / `TryTakeOrderedJob` |
| 23 | **右键强制维修（修补台）**：先给 2～3 件装备挂修补标记 → 选中一个小人 → 右键点修补台 | 菜单列出全图每件带标记装备的「强制维修：<装备名>」；点其中一条，小人立刻去修那件，且优先用你点的这座台。若这座台修补剂不够，该条变灰并提示没有可用修补台（不会偷偷换去别的台） | 截图（右键台子的菜单 + 出发画面）；Player.log 搜 `Exception` |
| 24 | **音量对比**：让小人修一件装备，把镜头放在修补台旁；再让另一个小人去旁边原版锻造台/缝纫台干活，交替听 | 两边的**工作循环音音量在同一档**（修补台不更响），离开 10 格外都基本听不见；修好时那声扣合音只是略突出一点，不炸耳朵 | 录屏 10 秒（两个台子都响的时候）；若还是偏响/偏轻，说方向（改 `Defs/SoundDefs/RepairBench_Sounds.xml` 的 volumeRange） |
| 25 | **装了 Yayo 就用 Yayo 的动作**：保持 Yayo 启用（它在你 activeMods 里）→ 让小人修一件装备，同时让另一个小人去旁边原版锻造台做东西 | 修补的小人做的是 **Yayo 的动作**（程序化前后俯仰/抬手，和旁边在锻造台干活的小人同款），**不是**本 mod 的 ±7.5° 锉磨晃动；不叠加、不抽搐；修完/打断后立刻恢复正常站姿。启动日志里应有一条 `[Repair Bench] Yayo's Animation detected`。Player.log 里**不应**出现与 yayo 或 Animation 相关的 Exception | 录屏 5～10 秒（两个小人同框）；Player.log 搜 `Repair Bench` / Exception / yayo。另可临时禁用 Yayo 再修一次，确认退化成本 mod 的 ±7.5° 晃动（说明自动回退也在工作） |

补充：
- 第 3 条若消耗修补剂数量与「缺失耐久 × 10」不符，请记录：修前 HP/最大 HP、台上修补剂前后数量。
- 第 6 条如果进度归零，说明 MapComponent 进度没接上（已知风险 R12）。
- 第 9 条右键菜单只在**有小人被选中**时出现（原版机制）；没有小人被选中时不会显示。不需要小人时用第 21 条的命令栏按钮。
- 第 19 条的根因：台子从原版 `BuildingBase` 继承了 `drawerType = MapMeshOnly`，这种绘制模式下 comp 的 `PostDraw` 根本不会被调用，所以原版 `CompRefuelable` 的缺料脉冲图标画不出来；已改成原版工作台用的 `MapMeshAndRealTime`。
- 第 21 条的按钮是通过 Patch 往原版抽象基类 `BaseWeapon` 与 `ApparelNoQualityBase` 的 comps 里追加自定义 comp 实现的（无 Harmony）。若某个第三方 mod 的装备没有这个按钮，说明它的 Def 没派生自这两个原版基类，属已知边界，不是本 mod 的 bug。
- 第 10 条已修过两次：最早单张 PNG + `Graphic_StackCount` 会让游戏按文件夹找图而报红字；后来改成 `RepairKit_a/_b/_c/_d.png` 四张堆叠图；现按你的要求固定成**单张** `RepairKit.png` + `Graphic_Single`（任何堆叠数量同一张图）。
- 第 8 条中文曾全部失效：DefInjected 用了 `<ThingDef><defName><label>` 嵌套写法，1.7 只认扁平 `<defName.label>`（否则日志报 `Key lacks a dot` 全部拒收）；现已改成扁平 key，日志里的 `Translation data ... has N errors` 若仍有，请确认是否为本 mod（数一下是不是别人的汉化包）。
- 第 15 条的音效与粒子是本 mod 自己合成的（`Sounds/RepairBench/RepairLoop.wav`、`RepairDone.wav`，金属屑 16×16 ×3）；觉得太响/太轻、屑片太大/太小，告诉我方向（不是 bug，直接调参数）。
- 第 18 条对应的红字根因：台的 graphicData 里同时写了 shadowData 与 staticSunShadowHeight，1.7 认为两者冲突；已删 shadowData（原版缝纫台也没写）。
- 第 24 条的音量根因：原来把循环音写成 `volumeRange 46~58`、收尾音 62~72，而原版工作音是 10~12（`Recipe_Smith` 12、`Recipe_Tailor` 10），等于响了 4～5 倍；现已改成循环音 10~12、收尾音 14~18，并补上原版同款 `distRange 10~50`。verify 里加了 `C-SoundVol` 契约（最大音量 > 20 直接 FAIL）防回归。
- 第 20 条与「别的台有工作动画」的解释：**原版没有任何工作动画**（`Toils_Recipe.DoRecipeWork` 只有特效/音效/进度条），你看到的动作来自 **Yayo's Animation**——它按 `CurJob.def.defName` 的硬编码白名单扭小人。所以本 mod 分两种模式：装了 Yayo 时用 Harmony prefix 把我们的 JobDef 改写成名单里的 `Repair`，直接复用它的动作（第 25 条）；没装 Yayo 时用 1.7 原生 AnimationDef 自己动（只转 Root 节点，帽子和衣服跟着一起动，不会错位）。
- 第 22/23 条：原版「优先执行工作」那个右键选项已被 `<directOrderable>false</directOrderable>` 关掉（它会拼出英文式 verb 文案），统一由本 mod 出「强制维修」，中英文一致。
- 第 25 条的源码依据：Yayo 的程序化动画每帧先查 `PawnRenderer.HasAnimation`（Yayo 源码 `AnimationCore.cs:37`），发现小人有 AnimationDef 动画就把自己那一帧的数据整体 `Reset()` 让位——所以「我们挂动画」和「用 Yayo 动画」只能二选一。现在的做法（`Source/RepairBench/RepairBenchYayoCompat.cs`）：启动时反射找 `YayoAnimation.AnimationCore` 与 `HarmonyLib.Harmony`，给 `AniStanding` 挂 prefix 把 `RepairBench_RepairItem` 改写成 `Repair`；桥接成功（`UseOwnAnimation == false`）就不挂我们的 AnimationDef。全反射、无硬依赖，任何一步失败只打 Warning 并回退本 mod 动画。verify 的 `C-JobFx`（`HasAnimation`/`ownsWorkAnimation`）与新增 `C-Yayo` 契约都做了断言防回归。
- 已知 v1 限制（不算 bug）：穿戴中装备不能标记、修完不自动归仓、不能修建筑、修补剂图标单张。
