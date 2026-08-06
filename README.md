# GunbreakerMod

FFXIV 绝枪战士（Gunbreaker）主题的 Slay the Spire 2 角色模组，基于 [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) 框架开发。卡牌设计以 `cards_gnb_updated.xlsx` 为唯一权威来源，本文档跟踪实装进度并记录踩过的坑。

## 项目结构

```
GunbreakerMod/
  GunbreakerModCode/
    Cards/        卡牌实现（每张卡一个 .cs 文件）
    Powers/       自定义 Power（力量/减益等状态效果）
    Characters/   角色定义（Gunbreaker.cs）
    Relics/       遗物
    Resources/    自定义副资源（晶壤/Cartridge）
    Vfx/          战斗内视觉效果钩子（如攻击前冲动画）
  GunbreakerMod/
    images/       美术资源（card_portraits = 卡面裁剪图，skill_icons = 原始下载图）
    localization/ 本地化文本（eng / zhs 两套）
cards_gnb_updated.xlsx   卡牌设计表格（唯一权威数据源）
image/                   角色美术源文件暂存
```

## 开发须知

- 构建前确认游戏未在运行：`Get-Process -Name "SlayTheSpire2" -ErrorAction SilentlyContinue`（PowerShell）
- 构建命令：`dotnet build`（在 `GunbreakerMod/` 目录下）
- 日志位置：`%APPDATA%\SlayTheSpire2\logs\godot.log`，每次启动游戏会话滚动一次，排查问题的第一手资料
- 反编译核对 API 用的 scratch 项目基于 `ICSharpCode.Decompiler`，指向本机 `sts2.dll` 与 NuGet 缓存里的 `STS2-RitsuLib.dll`；必要时也会反编译其他基于 RitsuLib 的创意工坊模组（如 Watcher、Squall）找参考实现

## 核心机制

- **晶壤 (Cartridge)**：独立于能量的副资源，上限 3，战斗内不清空、跨战斗重置。战斗界面用 3 个菱形图标显示在能量球正上方（未点亮=灰，点亮=蓝，带白色描边），见 `Resources/CartridgeResource.cs`。
- **续剑 (Continuation)**：打出【续剑】后获得隐藏的 `ContinuationPower`。此后每张消耗晶壤的卡在自己的 `OnPlay` 里检查 `HasPower<ContinuationPower>()`，额外生成对应续剑 token；没有统一的全局钩子，每条连击链自己维护映射。续剑拥有后自动从奖励/商店池排除。
- **终结连击**：打出【终结击】（Terminal Trigger）后在抽牌堆顶部放入【崛起之心】，打出后再放入【支配之心】，再放入【终结之心】——严格顺序链条，不是三选一分支，每张都对主目标造成高伤害、对其余敌人造成溅射伤害。
- **攻击牌前冲动作**：角色没有 Spine 骨骼动画（纯静态图），原版 `SetAnimationTrigger("Attack")` 对我们的立绘是空操作。改为在 `Vfx/AttackLungeListener.cs` 里注册全局的 `ICardOnPlayHookListener`：Gunbreaker 打出 Attack 类型卡时，用 Godot `Tween` 让 `NCreature.Visuals`（只含立绘本体，不含血条/UI）前冲再弹回，不阻塞卡牌本身的伤害结算。

## 卡牌进度

✅ = 已实装，📝 = 表格中标记"草稿"、设计未最终确定（暂不实装）。全部非草稿卡已实装完毕。

### 基础 / 爆发窗口
| Key | 名称 | 备注 |
|---|---|---|
| Strike_GNB | 打击 | 初始卡 x3 |
| Defend_GNB | 防御 | 初始卡 x4，复用原版铁甲战士卡面 |
| NoMercy | 无情 | 初始卡 x1 |

### 基础连击链
| Key | 名称 | 备注 |
|---|---|---|
| KeenEdge | 利刃斩 | 初始卡 x1 |
| BrutalShell | 残暴弹 | 连击生成 |
| SolidBarrel | 迅连斩 | 连击生成，获得1档晶壤 |

### 续剑核心
| Key | 名称 | 备注 |
|---|---|---|
| Continuation | 续剑 | Uncommon，自动排除重复 |
| BurstStrike | 爆发打击 | 初始卡 x1，晶壤满时自动回手牌 |
| Hypervelocity | 超高速 | 爆发打击续剑 token |

### 晶壤消耗链：烈牙连
| Key | 名称 | 备注 |
|---|---|---|
| GnashingFang | 烈牙 | Uncommon |
| SavageClaw | 猛兽爪 | 连击生成 |
| WickedTalon | 凶禽爪 | 连击生成（链末端） |
| JugularRip | 撕喉 | 烈牙续剑 token |
| AbdomenTear | 裂膛 | 猛兽爪续剑 token |
| EyeGouge | 穿目 | 凶禽爪续剑 token |

### 晶壤消耗链：倍攻 / 血壤 / 终结连
| Key | 名称 | 备注 |
|---|---|---|
| DoubleDown | 倍攻 | Rare，Exhaust，不接续剑逻辑 |
| Bloodfest | 血壤 | Common，纯获得晶壤 |
| TerminalTrigger | 终结击 | Rare（原【终结技】Finisher 改名，效果不变） |
| ReignOfBeasts | 崛起之心 | 主目标+溅射，生成支配之心 |
| NobleBlood | 支配之心 | 主目标+溅射，生成终结之心 |
| LionHeart | 终结之心 | 主目标+溅射（链末端） |

### AoE 连击链
| Key | 名称 | 备注 |
|---|---|---|
| DemonSlice | 恶魔切 | Common |
| DemonSlaughter | 恶魔杀 | 连击生成，获得1档晶壤 |
| FatedCircle | 命运之环 | Uncommon |
| FatedBrand | 命运之印 | 命运之环续剑 token |

### 机动 / 远程
| Key | 名称 | 备注 |
|---|---|---|
| LightningShot | 闪雷弹 | Common |
| RoughDivide | 粗分斩 | Common |
| Trajectory | 弹道 | Common |

### 高伤 / DoT
| Key | 名称 | 备注 |
|---|---|---|
| BlastingZone | 爆破领域 | Uncommon |
| SonicBreak | 音速破 | Common |
| BowShock | 弓形冲波 | Common |

### 减伤 / 生存
| Key | 名称 | 备注 |
|---|---|---|
| RoyalGuard | 王室亲卫 | Uncommon，被攻击时该敌人下回合失去力量（非永久） |
| Rampart | 铁壁 | Uncommon |
| Nebula | 星云 | Rare |
| HeartOfLight | 光之心 | Uncommon |
| Camouflage | 伪装 | Uncommon，格挡伤害反弹给敌人 |
| HeartOfStone | 石之心 | Common |
| HeartOfCorundum | 刚玉之心 | Uncommon |
| Superbolide | 超火流星 | Rare，消耗2档晶壤，扣血后本回合免疫伤害 |
| Aurora | 极光 | Common，消耗1档晶壤 |
| Reprisal | 雪仇 | Common |
| ArmsLength | 亲疏自行 | Common，Exhaust，获得人工制品 |
| SoulOfAzure | 灵魂之青 | Rare Power，回合开始获得缓冲（逐回合累加）+1档晶壤 |

### 草稿区（暂不实装）
EnergyRelease、RapidReload、EmptyMag、Trigger、Overcharge、MagazineExpansion、Roulette、EtherConversion、IntegratedImpact、SuppressingFire、CasingRecovery、TacticalReload、FullMagazine

**统计**：表格总计 43 张确认卡，已全部实装；草稿区 13 张暂不处理。

## 遗物 / 药水

- `GunbreakerStarterRelic`：占位实现，效果待设计。
- 遗物池/药水池目前只有占位内容，尚未按设计补充。

## 美术资源现状

- 【打击】【终结击】卡面、角色战斗立绘（`gunbreaker_char.png`，静态图 287×400，暂无动画）已替换为用户绘制的正式 STS2 风格美术。
- 【防御】直接复用原版铁甲战士卡面，不单独绘制。
- 其余卡牌仍是 FF14 官方技能图标裁剪的占位图，待后续按 STS2 风格逐步替换。

## 已修复的问题

按时间顺序记录踩过的坑，技术类（引擎/框架）和数值/平衡类问题都记在这里，不放进"核心机制"。

### 2026-08-04

- **本地化文本完全不生效**：manifest 里 `has_pck` 默认 `false` 导致本地化表未加载。改为 `true` 后修复。

### 2026-08-05

- **选完角色又变回铁甲战士**：误将 `RequiresEpochAndTimeline` 设为 `false`（实际含义是"完全不接入纪元/飞升系统"，只有不走正常角色选择界面的角色才该设为 `false`）。恢复默认 `true` 后修复。
- **数值不随力量/易伤等加成变色**：本地化占位符必须写成 `{VarName:diff()}` 才会触发高亮着色，裸 `{VarName}` 只走普通 `ToString()`，永远是静态白色。
- **领奖励/进商店抛异常卡死**：奖励卡稀有度摇点只在 Common/Uncommon/Rare 循环（`CardFactory.CreateForReward`），三者加起来不足 3 张时摇不出候选直接抛异常。卡池必须始终保有 ≥3 张 Common 及以上的卡。
- **进商店直接黑屏卡死（读档也一样）**：商店要为 Attack/Skill/Power 各摇一张卡上架；续剑当时是卡池里唯一的 Power 卡，拥有后按设计自我排除，导致 Power 池归零直接抛异常。新增第二张 Power 卡（王室亲卫）后修复——"拥有后自我排除"的唯一卡都要确认其 `CardType` 在池中还有其他候选。
- **消耗晶壤的卡可以在没有晶壤时被免费打出**：`this.SecondaryCosts().Set(...)` 要放在 `AfterCloned()`（每次克隆都会执行）里设置；`AfterCreated()` 只在特定创建路径触发，战斗里实际使用的克隆实例往往漏掉费用声明。受影响卡：爆发打击、极光、烈牙、倍攻。

### 2026-08-06

- **自定义晶壤图标节点导致整个战斗界面崩溃（无人物/无血条/无晶壤槽）**：把图标行写成自定义 `PipRow : HBoxContainer` 子类并覆盖 `_Ready()`/`_Process()`，导致 MonoMod 热补丁在每次进战斗时抛异常，打断整个战斗房间搭建流程。**结论：战斗内自定义 UI 只用内置节点类型拼装（HBoxContainer/TextureRect/Timer 等），不要继承 Node 覆盖生命周期方法。**
- **换上正式角色立绘后人物和血条又消失**：受击框尺寸直接按贴图像素尺寸计算，也决定战斗中的显示大小；原图 1792×2496 太大导致这部分数值计算出错（`NHealthBarGraftUiPatchHelper.SyncHpBarToHitbox` 报"数值不是有限数"）。缩小到 287×400 后修复，实际显示画质无明显损失。
- **晶壤图标"开局不显示"，`CallDeferred` 没能真正修好**：一次性补救对时序很敏感、不稳定。改为挂一个内置 `Godot.Timer`（0.2秒一次），每次都用 `CombatManager` 当前状态重新计算是否该显示，不再依赖某一次刷新是否踩对时序，最多0.2秒内自我纠正。
- **终结连击的溅射伤害打不到所有其他敌人**：场上3个及以上敌人时手写循环对每个敌人分别 `.Targeting(单个).Execute()`，只命中一个目标。改用 RitsuLib 提供的 `AttackCommandTargetingExtensions.TargetingFiltered`，把溅射伤害当成一次性命令传入过滤好的目标列表后修复。
- **终结连击生成的续卡完全看不到**：表面原因是新生成的卡直接进抽牌堆没有过场动画（`CardPileCmd` 只对"经过手牌/出牌区"或"在抽牌堆/弃牌堆/消耗堆之间互相移动"这两类换堆播放动画），改成先生成进手牌、再移到抽牌堆顶部后，问题依然存在。回头查 `godot.log` 才发现真正根因：溅射目标用的是 `CombatState.GetOpponentsOf(...).Where(...)` 惰性查询，绑定的是敌人列表的活引用；溅射打死目标时列表被实时修改，惰性查询枚举到一半抛出 `InvalidOperationException: Collection was modified`，直接中断整个 `OnPlay`，后面生成续卡的代码根本没执行到。加 `.ToList()` 把目标列表固化成快照后修复。

## 待跟进事项

- 卡牌边框颜色 `#01FCFE` 的着色器实验（`GunbreakerCardPool.PoolFrameMaterial`）未确认生效，用户表示暂不修复。
