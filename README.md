# Repair Bench（修补台）

RimWorld 1.7 mod —— 用命令标记把磨损的武器 / 衣物 / 护甲送到修补台，消耗通用「修补剂」修回 100% 耐久。

- packageId：`redbean.repairbench`
- modVersion：0.1.0
- 支持版本：RimWorld 1.7
- 作者：Redbean

## 玩法

1. 研究「修补技术」（Medieval 时代，600 点，前置「锻造」）解锁修补台与标记。
2. 造一张**修补台**（3×1，钢材 75 + 布料 15），在旁边用「制作修补剂」配方做**修补剂**
   （钢材 10 + 布料 5 → 5 个；可在燃料锻造台 / 电力锻造台制作）。
3. 把修补剂搬到修补台上（和原版给燃料锻造台加燃料一样），一台最多装 20 个。
4. 在「命令」菜单点「修补」，框选或点击地上磨损的武器 / 衣物 / 护甲，物品会挂上修补标记。
5. 有「修补」工作的小人会依次搬起装备 → 放到修补台上 → 按缺失耐久消耗修补剂并把它修满，
   修好后物品留在台旁地上（不会自动回仓库）。

修满一次大约需要 3000 工作单位，按缺失耐久折算：**每缺失 10% 耐久 ≈ 1 个修补剂、300 工作**。
工作速度吃「综合劳动速度」（GeneralLaborSpeed），并受手工技能经验（每 tick +0.1 Crafting）。
中途被打断不丢进度：进度按物品记录，回来接着修。

## 提示与 Alert

地图上出现「已标记但要修不了」的情况时会显示**提示（Alert）**：没有可用修补台，或台上修补剂不够。
点击 Alert 可以跳到相关物品。

## 已知限制（v1）

- **只修地上的装备**。穿在身上 / 拿在手里的装备没有「脱下」命令，无法标记；请先让小人丢下。
- 修完**不自动归仓**，物品留在修补台旁。
- 不修补建筑（建筑请用原版「修理」工作）。
- 不支持堆叠物品：堆叠数 > 1 的装备无法标记（武器/护甲正常情况都是单件）。
- 未做品质、耐久上限的额外加成：修满即 100%，品质 / 材质 / 词条保持原样。

## 兼容性

- 只做 XML Def + 1 个补丁（往原版「命令」分类追加设计器）+ 独立 DLL，不改原版 Def 数值。
- 与「修理建筑」的原版系统、各类工作类型 mod 不冲突；新增工作类型「修补」默认激活，可在工作优先级里调整。
- 存档：新增自定义 MapComponent 记录修理进度；**中途移除本 mod 未验证**（建议先修完再移除）。

## 安装

把 `Repair Bench` 文件夹放进 `RimWorld/Mods/`，在游戏内启用。需要 RimWorld 1.7。

## 开发

```powershell
# 编译（注意：需要 .NET 10 SDK，系统只装了 9 时用用户目录的 dotnet）
& "$env:USERPROFILE\.dotnet\dotnet.exe" build .\Source\RepairBench\RepairBench.csproj -c Release

# 静态自检（16 项契约：Def 字段、命名、语言、贴图路径、DLL 元数据等）
powershell -NoProfile -ExecutionPolicy Bypass -File tools\verify.ps1 -Strict

# 部署到游戏目录（会先结束正在运行的 RimWorld）
powershell -NoProfile -ExecutionPolicy Bypass -File tools\deploy.ps1

# 打包发布版（不含 Source）
powershell -NoProfile -ExecutionPolicy Bypass -File tools\make-release.ps1
```

数值都在 XML 里（`Defs/ThingDefs_Buildings/RepairBench_Bench.xml` 的 modExtension：
`ticksForFullRepair` / `kitsForFullRepair`），改完不需要重新编译。

## 致谢与参考

同期类似的装备修理实现（思路参考，非代码来源）：NanoRepairTech、MendAndRecycle、Repairable Gear。
美术：扁平矢量风自绘（Pillow 程序化绘制），对齐原版建筑贴图的描边 / 三阶平涂 / 俯视 2.5D 规范。
