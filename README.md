# GunbreakerMod

FFXIV 绝枪战士（Gunbreaker）主题的 Slay the Spire 2 角色模组，基于 [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) 框架开发。卡牌设计以 `cards_gnb_updated.xlsx` 为唯一权威来源，本文档跟踪对照表格的实装进度。

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

## 核心机制说明

- **晶壤 (Cartridge)**：独立于能量的副资源，上限 3，战斗内不清空、跨战斗重置。战斗界面用 3 个菱形图标显示（未点亮=灰，点亮=蓝），位置固定在能量球正上方（见 `Resources/CartridgeResource.cs`）。
- **续剑 (Continuation)**：打出【续剑】后获得 `ContinuationPower`（隐藏标记，无独立图标）。此后每张会消耗晶壤的卡牌在自己的 `OnPlay` 里各自检查 `HasPower<ContinuationPower>()`，额外生成一张对应的续剑token。没有统一的全局钩子，每条连击链自己维护映射关系。续剑本身在拥有后会从奖励池/商店池自动排除（不会抽到第二张）。
- **动态数值染色**：本地化文本里数值占位符必须写成 `{VarName:diff()}`（注意括号），才会在力量/易伤等加成生效时变绿、减益时变红。裸的 `{VarName}` 只会显示静态白色数字。
- **卡牌池稀有度规则**：奖励/商店的稀有度摇点只在 Common/Uncommon/Rare 之间循环，Basic 和 Token 永远不会作为奖励出现。这意味着卡池必须始终保有一定数量的 Common 及以上卡牌，否则奖励结算会抛异常卡死。
- **商店额外要求按类型（Attack/Skill/Power）也要有余量**：商店固定会摇一张"Power类型"卡牌上架，如果卡池里唯一的Power卡（续剑）已经被玩家拿到（因而从商店池自我排除），卡池里就会变成"0张可上架的Power卡"，商店进入时会直接抛异常导致黑屏卡死（即使重新读档也一样，因为存档本身停在商店房间，每次读档都会重新触发）。2026-08-05 通过新增第二张Power卡（王室亲卫）修复。以后每新增一张会自我排除的"唯一卡"，都要检查其所属`CardType`在池子里是否还有其他候选。
- **AfterCloned() 而非 AfterCreated()**：凡是需要用 `this.SecondaryCosts().Set(...)` 声明副资源费用的卡牌，必须在 `AfterCloned()`（而不是 `AfterCreated()`）里设置——因为费用数据挂在按实例引用寻址的附加状态表上，而战斗内的卡牌实例是通过克隆产生的，只有 `AfterCloned()` 保证每次克隆都会重新执行。这是 2026-08-05 修复的一个严重bug（爆发打击/倍攻等卡曾经可以在没有晶壤时被免费打出）。

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
| DoubleDown | 倍攻 | ✅ Rare，不接续剑逻辑 |
| Bloodfest | 血壤 | ✅ Common，纯获得晶壤 |
| Finisher | 终结技 | ⬜ 未实装（需要三选一生成机制） |
| ReignOfBeasts | 崛起之心 | ⬜ 未实装（终结技分支token） |
| NobleBlood | 支配之心 | ⬜ 未实装（终结技分支token） |
| LionHeart | 终结之心 | ⬜ 未实装（终结技分支token） |

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
| RoyalGuard | 王室亲卫 | ✅ Uncommon，被攻击时反击敌人力量 |
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

**统计**：表格总计 55 张卡（不含草稿区 14 张），已实装 33 张，未实装 8 张（Finisher链4张 + BlastingZone + Nebula + HeartOfLight + Camouflage + Superbolide）。

## 遗物 / 药水

- `GunbreakerStarterRelic`：占位实现，效果待设计。
- 遗物池/药水池目前只有占位内容，尚未按设计补充。

## 已知的待跟进事项

- 卡牌边框颜色 `#01FCFE` 的着色器实验（`GunbreakerCardPool.PoolFrameMaterial`）未确认生效，用户表示暂不修复。
- 角色初始卡数量（KeenEdge=1、NoMercy=1、BurstStrike=1）均为设计表格未明确给出、由开发者暂定的数值，未经用户最终确认。
- 全部美术资源均为 FF14 官方技能图标裁剪的占位图，正式美术会按 STS2 风格重绘。

## 开发须知

- 构建前确认游戏未在运行：`Get-Process -Name "SlayTheSpire2" -ErrorAction SilentlyContinue`（PowerShell）
- 构建命令：`dotnet build`（在 `GunbreakerMod/` 目录下）
- 日志位置：`%APPDATA%\SlayTheSpire2\logs\godot.log`（每次启动游戏会话滚动一次）
- 反编译核对 API 时使用的 scratch 项目基于 `ICSharpCode.Decompiler`，指向本机安装的 `sts2.dll` 与 NuGet 缓存里的 `STS2-RitsuLib.dll`。
