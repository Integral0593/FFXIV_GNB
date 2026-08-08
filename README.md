# GunbreakerMod

FFXIV 绝枪战士（Gunbreaker）主题的 Slay the Spire 2 角色模组，基于 [STS2-RitsuLib](https://github.com/BAKAOLC/STS2-RitsuLib) 框架开发。卡牌设计以 `cards_gnb_updated.xlsx` 为唯一权威来源。

## 构建

日常改代码/图片/文案，在 `GunbreakerMod/` 目录下跑：

```bash
dotnet build
```

会编译 C# 并自动把资源快速打包进 `mods/GunbreakerMod/`。

**例外**：项目里只要有 `.tscn` 场景文件（目前是 `GunbreakerMod/scenes/energy_counter.tscn`），`dotnet build` 自带的快速打包工具（`BSchneppe.StS2.PckPacker`）就会跳过打包整个资源包（不支持场景文件）。这种情况下必须用 Godot 编辑器手动导出一次：

1. 用 Godot **4.5.1 Mono**（C# 版本，需与 `Godot.NET.Sdk` 版本一致）打开 `GunbreakerMod/project.godot`
2. `项目 → 导出... → BasicExport → 导出 PCK/ZIP...`
3. 导出的 `GunbreakerMod.pck` 覆盖到 `mods/GunbreakerMod/GunbreakerMod.pck`；DLL/PDB 仍由 `dotnet build` 生成，两者独立

构建前确认游戏未在运行：`Get-Process -Name "SlayTheSpire2" -ErrorAction SilentlyContinue`（PowerShell）。日志位置：`%APPDATA%\SlayTheSpire2\logs\godot.log`，每次启动游戏会话滚动一次。

反编译核对 API 用的 scratch 项目基于 `ICSharpCode.Decompiler`，指向本机 `sts2.dll` 与 NuGet 缓存里的 `STS2-RitsuLib.dll`；必要时也会反编译其他基于 RitsuLib 的创意工坊模组（如 Watcher、Squall）找参考实现。

## 项目结构

```
GunbreakerMod/
  GunbreakerModCode/
    Cards/        卡牌实现（每张卡一个 .cs 文件）
    Powers/       自定义 Power（力量/减益等状态效果）
    Characters/   角色定义（Gunbreaker.cs）
    Relics/       遗物
    Resources/    自定义副资源（晶壤/Cartridge）
    Vfx/          战斗内视觉效果（攻击前冲/受击后弹动画）
  GunbreakerMod/
    images/       美术资源（card_portraits = 卡面裁剪图，skill_icons = 原始下载图，powers/ = 状态图标）
    scenes/       手写的 Godot 场景（目前只有能量槽 energy_counter.tscn）
    localization/ 本地化文本（eng / zhs 两套）
cards_gnb_updated.xlsx   卡牌设计表格（唯一权威数据源）
image/                   角色美术源文件暂存
```

## 核心机制

- **晶壤 (Cartridge)**：独立于能量的副资源，上限 3，战斗内不清空、跨战斗重置。战斗界面用 3 个菱形图标显示在能量球正上方，见 `Resources/CartridgeResource.cs`。
- **续剑 (Continuation)**：打出【续剑】后获得隐藏的 `ContinuationPower`。此后每张消耗晶壤的卡在自己的 `OnPlay` 里检查 `HasPower<ContinuationPower>()`，额外生成对应续剑 token。续剑拥有后自动从奖励/商店池排除。
- **终结连击**：打出【终结击】（Terminal Trigger）后依次在抽牌堆顶部放入【崛起之心】→【支配之心】→【终结之心】，每张都对主目标造成高伤害、对其余敌人造成溅射伤害。
- **攻击/受击动作**：打出攻击牌时角色小幅前冲，受到伤害时向后弹开，再回到原位，见 `Vfx/CreatureBumpAnimator.cs`（Godot `Tween` 直接驱动立绘节点位置）。
- **起始遗物**：绝枪战士之证——每场战斗开始时获得 1 档晶壤，见 `Relics/GunbreakerStarterRelic.cs`。
- **副资源高亮文本**：卡牌/遗物描述里用 `{VarName:secondaryResourceIcons()}` 把晶壤渲染成图标+悬浮说明，效果与原版能量的展示方式一致。

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
| Superbolide | 超火流星 | Rare，消耗2档晶壤，扣血后本回合免疫所有伤害（非"抵挡一次"） |
| Aurora | 极光 | Common，消耗1档晶壤 |
| Reprisal | 雪仇 | Common |
| ArmsLength | 亲疏自行 | Common，Exhaust，获得人工制品 |
| SoulOfAzure | 灵魂之青 | Rare Power，回合开始获得缓冲（逐回合累加）+1档晶壤 |

### 草稿区（暂不实装）
EnergyRelease、RapidReload、EmptyMag、Trigger、Overcharge、MagazineExpansion、Roulette、EtherConversion、IntegratedImpact、SuppressingFire、CasingRecovery、TacticalReload、FullMagazine

**统计**：表格总计 43 张确认卡，已全部实装；草稿区 13 张暂不处理。

## 遗物 / 药水

- `GunbreakerStarterRelic`（绝枪战士之证）：战斗开始时获得 1 档晶壤，见上文核心机制。
- 遗物池/药水池目前只有这一件起始遗物，其余尚未按设计补充。

## 美术资源现状

- 已有正式 STS2 风格美术：角色战斗立绘、商人/休息处场景、能量槽背景、起始遗物图标、【打击】【终结击】卡面、三个可见 Power（无情/续剑/王室亲卫）的状态图标。
- 【防御】直接复用原版铁甲战士卡面。
- 其余卡牌仍是 FF14 官方技能图标裁剪的占位图，待后续按 STS2 风格逐步替换。

## 已知问题 / 修复记录

按时间顺序记录踩过的坑，只记根因和修法，用于避免重复踩坑。

### 2026-08-04

- **本地化文本完全不生效**：manifest 里 `has_pck` 默认 `false` 导致本地化表未加载。改为 `true` 后修复。

### 2026-08-05

- **选完角色又变回铁甲战士**：误将 `RequiresEpochAndTimeline` 设为 `false`（实际含义是"完全不接入纪元/飞升系统"，只有不走正常角色选择界面的角色才该设为 `false`）。恢复默认 `true` 后修复。
- **数值不随力量/易伤等加成变色**：本地化占位符必须写成 `{VarName:diff()}` 才会触发高亮着色，裸 `{VarName}` 只走普通 `ToString()`。
- **领奖励/进商店抛异常卡死**：奖励卡稀有度摇点只在 Common/Uncommon/Rare 循环，三者加起来不足 3 张时摇不出候选直接抛异常。卡池必须始终保有 ≥3 张 Common 及以上的卡。
- **进商店直接黑屏卡死（读档也一样）**：商店要为 Attack/Skill/Power 各摇一张卡上架；续剑当时是卡池里唯一的 Power 卡，拥有后按设计自我排除，导致 Power 池归零直接抛异常。新增第二张 Power 卡后修复——"拥有后自我排除"的唯一卡都要确认其 `CardType` 在池中还有其他候选。
- **消耗晶壤的卡可以在没有晶壤时被免费打出**：费用要在 `AfterCloned()`（每次克隆都会执行）里设置；`AfterCreated()` 只在特定创建路径触发，战斗里实际使用的克隆实例往往漏掉费用声明。

### 2026-08-06

- **自定义晶壤图标节点导致整个战斗界面崩溃**：把图标行写成自定义 `PipRow : HBoxContainer` 子类并覆盖 `_Ready()`/`_Process()`，导致 MonoMod 热补丁抛异常，打断整个战斗房间搭建流程。战斗内自定义 UI 只用内置节点类型拼装，不要继承 Node 覆盖生命周期方法。
- **换上正式角色立绘后人物和血条消失**：受击框尺寸直接按贴图像素尺寸计算；原图 1792×2496 太大导致计算出错。缩小到 287×400 后修复。
- **晶壤图标开局不显示**：一次性的 `CallDeferred` 补救对时序敏感、不稳定。改为挂一个 `Godot.Timer`（0.2秒一次）持续用 `CombatManager` 当前状态自我纠正，不再依赖某一次刷新的时序。
- **终结连击的溅射伤害打不到所有其他敌人**：手写循环对每个敌人分别调用攻击，只命中一个目标。改用 RitsuLib 的 `TargetingFiltered`，把溅射伤害当成一次性命令传入过滤好的目标列表后修复。
- **终结连击生成的续卡完全看不到**：真正根因是溅射目标用了惰性 `.Where()` 查询敌人活列表；溅射打死目标时列表被修改，惰性查询枚举到一半抛出 `InvalidOperationException`，直接中断整个 `OnPlay`，后面生成续卡的代码根本没执行到。加 `.ToList()` 固化目标列表后修复。

### 2026-08-07

- **超火流星"本回合免疫伤害"实际上只挡了一下**：最初用大量 `BufferPower` 叠层模拟无敌，但 Buffer 每挡一次伤害就消耗一层，同一回合被多个敌人攻击时形同虚设。改为参考原版 `IntangiblePower` 的做法——用 `ModifyHpLostAfterOsty` 把所有伤害无条件封顶为 0，只在敌方回合结束时清除一次，而不是按次数消耗，才是真正的"这回合不受伤害"。
- **超火流星的免疫在敌人出手前就被清空**：挂在 `AfterSideTurnEnd` 上清除，但这个时机在玩家自己回合结束时就会触发，敌人还没行动免疫就没了。改成参考原版做法在敌方回合结束时清除后修复。
- **生成的续卡预览是"先进手牌再移到抽牌堆"，效果具有误导性**：这个折中方案本身是基于一个记错的原版参考（以为储君的《决断》是这么做的，反编译后发现完全不是）。改为反编译多张原版生成卡（如 Turbo→Void）后，直接用 `CardPileCmd.AddGeneratedCardToCombat` 一步到位放入目标牌堆，再用 `CardCmd.PreviewCardPileAdd` 弹出专门的卡牌预览动画。

### 2026-08-08

- **战斗内常驻能量槽换皮不生效**：一直以为只是没接对资源覆盖接口，反编译后发现常驻能量槽是从角色专属的 `.tscn` 场景加载的，跟 `EnergyIconHelper` 之类的文本图标系统完全无关，纯代码手段做不到。最终用 Godot 编辑器手写并导出了一个新场景（见"构建"一节）。
- **能量槽位置怎么调都没反应**：`NEnergyCounter.AnimIn()` 每次进战斗都会把整个节点的 `Position` 强制重置为 `(0,0)`，改根节点位置必然被覆盖。改成对每个子节点单独加偏移（`AnimIn` 只重置根节点）后生效。
- **卡牌/遗物描述里的能量小图标巨大**：`TextEnergyIconPath` 直接指向了 256×256 的完整能量槽背景图，而富文本 `[img]` 标签没有尺寸限制，会按原图大小渲染。换成专门裁的 48×48 小图后修复。
- **晶壤高亮图标不显示，只剩数字**：`{Cartridge:secondaryResourceIcons}` 少写了一对括号，格式化器没有被识别，回退成了 `DynamicVar` 默认的 `ToString()`。改成 `{Cartridge:secondaryResourceIcons()}` 后修复，写法要跟 `:diff()` 保持一致。

## 待跟进事项

- 卡牌边框颜色 `#01FCFE` 的着色器实验（`GunbreakerCardPool.PoolFrameMaterial`）未确认生效。
