# GunbreakerMod

FFXIV 绝枪战士（Gunbreaker）主题的 Slay the Spire 2 角色模组，基于 [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) 框架开发。卡牌设计以 `cards_gnb_updated.xlsx` 为唯一权威来源，本文档跟踪对照表格的实装进度，并记录开发过程中踩过的坑。

## 项目结构

```
GunbreakerMod/
  GunbreakerModCode/
    Cards/        卡牌实现（每张卡一个 .cs 文件）
    Powers/       自定义 Power（力量/减益等状态效果）
    Characters/   角色定义（Gunbreaker.cs）
    Relics/       遗物
    Resources/    自定义副资源（晶壤/Cartridge）
  GunbreakerMod/
    images/       美术资源（card_portraits = 卡面裁剪图，skill_icons = 原始下载图）
    localization/ 本地化文本（eng / zhs 两套）
cards_gnb_updated.xlsx   卡牌设计表格（唯一权威数据源）
image/                   角色美术源文件暂存
```

## 开发须知

- 构建前确认游戏未在运行：`Get-Process -Name "SlayTheSpire2" -ErrorAction SilentlyContinue`（PowerShell）
- 构建命令：`dotnet build`（在 `GunbreakerMod/` 目录下）
- 日志位置：`%APPDATA%\SlayTheSpire2\logs\godot.log`（每次启动游戏会话滚动一次，排查问题时的第一手资料）
- 反编译核对 API 时使用的 scratch 项目基于 `ICSharpCode.Decompiler`，指向本机安装的 `sts2.dll` 与 NuGet 缓存里的 `STS2-RitsuLib.dll`；必要时也会反编译其他同样基于 RitsuLib 的创意工坊模组（如 Watcher、Squall）来找可参考的实现。

## 玩法设计：核心机制

- **晶壤 (Cartridge)**：独立于能量的副资源，上限 3，战斗内不清空、跨战斗重置。战斗界面用 3 个菱形图标显示（未点亮=灰，点亮=蓝，带白色描边），位置固定在能量球正上方（见 `Resources/CartridgeResource.cs`）。
- **续剑 (Continuation)**：打出【续剑】后获得 `ContinuationPower`（隐藏标记，无独立图标）。此后每张会消耗晶壤的卡牌在自己的 `OnPlay` 里各自检查 `HasPower<ContinuationPower>()`，额外生成一张对应的续剑token。没有统一的全局钩子，每条连击链自己维护映射关系。续剑本身在拥有后会从奖励池/商店池自动排除（不会抽到第二张）。
- **终结技连击**：打出【终结技】后在抽牌堆顶部放入【崛起之心】，打出后再放入【支配之心】，再放入【终结之心】。每张都对主目标造成高伤害、对其余敌人造成溅射伤害。

## 卡牌实装进度

对照表格分区，✅ = 已实装并可在游戏中获取，⬜ = 表格已定稿但未实装，📝 = 表格中标记"草稿"、设计未最终确定。

### 基础卡
| Key | 名称 | 状态 |
|---|---|---|
| Strike_GNB | 打击 | ✅ 初始卡 x3 |
| Defend_GNB | 防御 | ✅ 初始卡 x4 |

### 增伤/爆发窗口
| Key | 名称 | 状态 |
|---|---|---|
| NoMercy | 无情 | ✅ 初始卡 x1 |

### 基础连击链
| Key | 名称 | 状态 |
|---|---|---|
| KeenEdge | 利刃斩 | ✅ 初始卡 x1 |
| BrutalShell | 残暴弹 | ✅ 连击生成 |
| SolidBarrel | 迅连斩 | ✅ 连击生成，获得1档晶壤 |

### 续剑核心
| Key | 名称 | 状态 |
|---|---|---|
| Continuation | 续剑 | ✅ Uncommon，自动从奖励/商店池排除重复 |
| BurstStrike | 爆发打击 | ✅ 初始卡 x1，晶壤满时自动回手牌 |
| Hypervelocity | 超高速 | ✅ 爆发打击的续剑token |

### 晶壤消耗链：烈牙连
| Key | 名称 | 状态 |
|---|---|---|
| GnashingFang | 烈牙 | ✅ Uncommon |
| SavageClaw | 猛兽爪 | ✅ 连击生成 |
| WickedTalon | 凶禽爪 | ✅ 连击生成（链末端） |
| JugularRip | 撕喉 | ✅ 烈牙的续剑token |
| AbdomenTear | 裂膛 | ✅ 猛兽爪的续剑token |
| EyeGouge | 穿目 | ✅ 凶禽爪的续剑token |

### 晶壤消耗链：倍攻/血壤/狮心连
| Key | 名称 | 状态 |
|---|---|---|
| DoubleDown | 倍攻 | ✅ Rare，Exhaust，不接续剑逻辑 |
| Bloodfest | 血壤 | ✅ Common，纯获得晶壤 |
| Finisher | 终结技 | ✅ Rare，获得晶壤+在抽牌堆顶部放入崛起之心 |
| ReignOfBeasts | 崛起之心 | ✅ 主目标+溅射，在抽牌堆顶部放入支配之心 |
| NobleBlood | 支配之心 | ✅ 主目标+溅射，在抽牌堆顶部放入终结之心 |
| LionHeart | 终结之心 | ✅ 主目标+溅射（链末端） |

### AoE连击链
| Key | 名称 | 状态 |
|---|---|---|
| DemonSlice | 恶魔切 | ✅ Common |
| DemonSlaughter | 恶魔杀 | ✅ 连击生成，获得1档晶壤 |
| FatedCircle | 命运之环 | ✅ Uncommon |
| FatedBrand | 命运之印 | ✅ 命运之环的续剑token |

### 机动/远程
| Key | 名称 | 状态 |
|---|---|---|
| LightningShot | 闪雷弹 | ✅ Common |
| RoughDivide | 粗分斩 | ✅ Common |
| Trajectory | 弹道 | ✅ Common（表格曾误写Basic，已修正） |

### 高伤/DoT转化
| Key | 名称 | 状态 |
|---|---|---|
| BlastingZone | 爆破领域 | ⬜ 未实装 |
| SonicBreak | 音速破 | ✅ Common |
| BowShock | 弓形冲波 | ✅ Common |

### 减伤/生存
| Key | 名称 | 状态 |
|---|---|---|
| RoyalGuard | 王室亲卫 | ✅ Uncommon，被攻击时该敌人下回合失去力量（非永久） |
| Rampart | 铁壁 | ✅ Uncommon |
| Nebula | 星云 | ⬜ 未实装 |
| HeartOfLight | 光之心 | ⬜ 未实装 |
| Camouflage | 伪装 | ⬜ 未实装（需要"格挡伤害反弹"新机制） |
| HeartOfStone | 石之心 | ✅ Common |
| HeartOfCorundum | 刚玉之心 | ✅ Uncommon |
| Superbolide | 超火流星 | ⬜ 未实装（需要"本回合免疫伤害"机制，待调研对应Power） |
| Aurora | 极光 | ✅ Common |

### 运转端（能力牌 / 技能攻击牌 / 抽牌）
表格标记为"草稿"，设计尚未最终确定，全部 📝 暂不实装：
EnergyRelease、RapidReload、EmptyMag、Trigger、Overcharge、SoulOfAzure、
MagazineExpansion、Roulette、EtherConversion、IntegratedImpact、SuppressingFire、
CasingRecovery、TacticalReload、FullMagazine

**统计**：表格总计 40 张卡（不含草稿区 14 张），已实装 35 张，未实装 5 张：BlastingZone、Nebula、HeartOfLight、Camouflage、Superbolide。

## 遗物 / 药水

- `GunbreakerStarterRelic`：占位实现，效果待设计。
- 遗物池/药水池目前只有占位内容，尚未按设计补充。

## 美术资源现状

- 【打击】【终结技】卡面、角色战斗立绘（`gunbreaker_char.png`，暂无动画，静态图，287x400）已替换为用户绘制的正式 STS2 风格美术。
- 【防御】直接复用原版铁甲战士的 Defend 卡面（`res://images/atlases/card_atlas.sprites/ironclad/defend_ironclad.tres`），不单独绘制。
- 其余卡牌仍是 FF14 官方技能图标裁剪的占位图，待后续按 STS2 风格逐步替换。

## 已修复的问题

按时间顺序记录遇到过的bug和根因，避免以后重复踩坑。技术类问题（引擎/框架层面）和数值/平衡类问题都记在这里，不放进"核心机制"章节。

### 2026-08-04

- **本地化文本完全不生效**：manifest 里 `has_pck` 默认是 `false`，导致本地化表根本没有被加载（日志报 `GetRawText: Key '...' not found in table`）。启用 `BSchneppe.StS2.PckPacker` 并把 `has_pck` 设为 `true` 后修复。

### 2026-08-05

- **角色选择后又变回铁甲战士**：把 `RequiresEpochAndTimeline` 设成了 `false`，以为这只是"跳过 Ancient 剧情"，实际上它的含义是"完全不接入游戏的纪元/飞升系统"——只有不通过正常角色选择界面的角色才该设为 `false`。日志显示 `SelectCharacter_Patch4` 在这个检查附近抛 `ArgumentOutOfRangeException`，选择被静默中断，画面停留在切换前的角色上。删掉这个覆盖、恢复框架默认值（`true`）后修复。
- **卡牌数值不会随力量/易伤等加成动态变色**：本地化文本里的数值占位符必须显式写成 `{VarName:diff()}`（注意要带括号）才会触发原版的高亮染色格式化器；裸的 `{VarName}` 只会走普通的 `ToString()`，永远是静态白色。反编译 `LocString.GetFormattedText()` 的官方注释里就直接给出了这个写法作为例子。
- **打出奖励/商店进入战斗结算会抛异常卡死**：奖励卡的稀有度摇点只在 Common/Uncommon/Rare 之间循环（`CardFactory.CreateForReward` 的 `GetNextAllowedRarity`），Basic 和 Token 永远不会被摇到。当时卡池里这三个稀有度的卡加起来不够 3 张（一次奖励要摇 3 选 1），第三次摇的时候找不到候选，直接抛 `InvalidOperationException`。补充了几张 Common/Uncommon 卡后修复。这条规则以后加卡也要注意：卡池必须**始终**保有至少 3 张 Common 及以上的卡。
- **进商店直接黑屏卡死（读档也一样）**：商店固定要为每种 `CardType`（Attack/Skill/Power）摇一张卡上架。续剑当时是卡池里唯一的 Power 卡，玩家拿到后它会（按设计）自我排除出奖励/商店池，导致 Power 类型在商店池归零，`CardFactory.CreateForMerchant` 直接抛异常。因为存档就停在商店房间，每次读档都会在同一个地方再崩一次。新增第二张 Power 卡（王室亲卫）后修复——这类"拥有后自我排除"的唯一卡，都要确认它所属的 `CardType` 在卡池里还有其他候选。
- **消耗晶壤的卡可以在没有晶壤时被免费打出**：`this.SecondaryCosts().Set(...)` 设置的费用数据是挂在一个按卡牌实例引用寻址的附加状态表上的，不会随着卡牌克隆自动复制。当时在 `AfterCreated()` 里设置费用，但这个钩子只在特定的创建路径上才会被调用；战斗里实际使用的卡牌实例是通过克隆产生的，很多时候克隆后费用声明就丢了。改成在 `AfterCloned()`（游戏真正的"每次克隆都会执行"的钩子）里设置后修复，受影响的卡：爆发打击、极光、烈牙、倍攻。

### 2026-08-06

- **自定义晶壤图标节点导致整个战斗界面崩溃（无人物、无血条、无晶壤槽）**：为了修复晶壤图标"开局不显示"的时序问题，把图标行改写成了一个自定义 `PipRow : HBoxContainer` 子类并覆盖 `_Ready()`/`_Process()`。这个节点类导致 MonoMod 的 JIT 钩子在每次进入战斗时直接抛异常，异常一路冒泡打断了整个战斗房间的搭建流程（`NRun.SetCurrentRoom`），不只是晶壤图标，人物立绘和血条都因此消失。根因没有完全查清（疑似自定义节点子类的虚方法覆盖和这套魔改环境下的 MonoMod 热补丁有冲突）。**结论：战斗内自定义UI节点只用普通的内置节点类型拼装（HBoxContainer/TextureRect/Control/Timer等），不要继承 Node 子类去覆盖生命周期方法。**
- **替换正式角色立绘后人物和血条又消失了**：换上用户绘制的 `gunbreaker_char.png`（原始 1792x2496）后触发了另一个独立的崩溃——`NHealthBarGraftUiPatchHelper.SyncHpBarToHitbox` 在计算血条对齐受击框尺寸时报"数值不是有限数"的引擎级错误。反编译 `RitsuNCreatureVisualsNodeFactory.FromTexture` 确认：一张普通PNG被自动包装成 `NCreatureVisuals` 时，受击框尺寸直接用图片的像素尺寸算出，也直接决定了在战斗中的显示大小——图片太大不仅显示会异常大，还会让这部分尺寸计算出错。分两步缩小到最终 287x400（长边约400px，接近框架里 Bounds 槽位的默认兜底尺寸 240x280 这个量级）后修复，画质在战斗中的实际显示尺寸下没有明显损失。
- **晶壤图标"开局不显示"，用 `CallDeferred` 没能真正修好**：一开始以为是 RitsuLib 在"刚注册完挂载节点"后会同步隐藏一次、而战斗开始时的首次刷新可能抢跑在这次隐藏之前完成，所以加了一次性的 `CallDeferred(Show)` 作为补救，但实测仍然不稳定。改为在图标行上挂一个内置的 `Godot.Timer` 节点（每0.2秒触发一次），每次都用 `CombatManager.Instance.DebugOnlyGetState()` + `LocalContext.GetMe(...)` 直接重新取得当前玩家、重新计算是否该显示，不再依赖 RitsuLib 内部那次"结算UI是否注册好了"的时序是否踩对点——这样无论一开始那次刷新有没有生效，最多0.2秒内都会被这个定时器自我纠正回正确状态。
- **终结技连击的溅射伤害打不到所有其他敌人**：场上3个及以上敌人时，崛起之心/支配之心/终结之心的溅射伤害只会命中一个"其他目标"。原因是用了一个手写循环、对每个敌人分别调用 `.Targeting(单个敌人).Execute(...)`——反编译发现 RitsuLib 其实提供了专门给"主目标+溅射到其余所有敌人"这种形状用的 API：`AttackCommandTargetingExtensions.TargetingFiltered`，把整个溅射伤害当成一次性的单个命令、传入一份过滤好的目标列表执行。换用这个 API 后修复。
- **终结技连击生成的续卡完全看不到**：崛起之心/支配之心/终结之心被放到抽牌堆顶部时，玩家看不到发生了什么。反编译 `CardPileCmd` 里挑选过场动画的逻辑发现：只有"从手牌/出牌区进/出"或者"在抽牌堆/弃牌堆/消耗堆之间互相移动"这两类换堆才会有对应的过场动画；一张全新生成、此前没有任何"旧堆"的卡直接进抽牌堆，两类都不匹配，所以完全没有任何视觉反馈。改成先生成进手牌（复用其他连击链已经在用、效果正常的"飞入手牌"过场），再立刻把这张卡移动到抽牌堆顶部（这一步换堆能正常触发过场动画）——这个"先进手牌、再移到抽牌堆顶部"的组合打法也是参考了原版储君角色的《决断》（DecisionsDecisions）卡的做法。

## 待跟进事项

- 卡牌边框颜色 `#01FCFE` 的着色器实验（`GunbreakerCardPool.PoolFrameMaterial`）未确认生效，用户表示暂不修复。
