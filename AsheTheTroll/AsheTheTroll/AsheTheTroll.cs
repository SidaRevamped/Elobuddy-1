﻿using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Color = System.Drawing.Color;

namespace AsheTheTroll
{
    internal class AsheTheTroll
    {
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static AIHeroClient _target;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static Spell.Active _q;
        public static Spell.Skillshot _w;
        public static Spell.Skillshot _e;
        public static Spell.Skillshot _r;
        public static Spell.Active Heal;
      
        private static readonly Vector2 Baron = new Vector2(5007.124f, 10471.45f);
        private static readonly Vector2 Dragon = new Vector2(9866.148f, 4414.014f);

        public static float HealthPercent
        {
            get { return _Player.Health/_Player.MaxHealth*100; }
        }

        private static Item HealthPotion;
        private static Item CorruptingPotion;
        private static Item RefillablePotion;
        private static Item TotalBiscuit;
        private static Item HuntersPotion;
        public static Item Youmuu = new Item(ItemId.Youmuus_Ghostblade);
        public static Item Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
        public static Item Cutlass = new Item(ItemId.Bilgewater_Cutlass);
        public static Item Tear = new Item(ItemId.Tear_of_the_Goddess);
        public static Item Qss = new Item(ItemId.Quicksilver_Sash);
        public static Item Simitar = new Item(ItemId.Mercurial_Scimitar);


        public static Menu Menu,
            ComboMenu,
            VolleyMenu,
            HarassMenu,
            JungleLaneMenu,
            MiscMenu,
            DrawMenu,
            ItemMenu,
            SkinMenu,
            AutoPotHealMenu,
            FleeMenu;

        private static void VolleyLocation(ValueBase<bool> sender, ValueBase<bool>.ValueChangeArgs args)
        {
            Volley.RecalculateOpenLocations();
        }


        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Ashe)
            {
                return;
            }


            _q = new Spell.Active(SpellSlot.Q);
            _w = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Linear, 0, int.MaxValue, 60);
            {
                _w.AllowedCollisionCount = 0;
            }
            _e = new Spell.Skillshot(SpellSlot.E, 15000, SkillShotType.Linear, 0, int.MaxValue, 0);
            _r = new Spell.Skillshot(SpellSlot.R, 15000, SkillShotType.Linear, 500, 1000, 250);
            _r.AllowedCollisionCount = int.MaxValue;
            var slot = _Player.GetSpellSlotFromName("summonerheal");
            if (slot != SpellSlot.Unknown)
            {
                Heal = new Spell.Active(slot, 600);
            }
            HealthPotion = new Item(2003, 0);
            TotalBiscuit = new Item(2010, 0);
            CorruptingPotion = new Item(2033, 0);
            RefillablePotion = new Item(2031, 0);
            HuntersPotion = new Item(2032, 0);
            Teleport.OnTeleport += Teleport_OnTeleport;

          
         Chat.Print(
                "<font color=\"#4dd5ea\" >MeLoDag Presents </font><font color=\"#ffffff\" >AsheTheToLL </font><font color=\"#4dd5ea\" >Kappa Kippo</font>");


            Menu = MainMenu.AddMenu("AsheTheTroll", "AsheTheTroll");

            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Q Settings:");
            ComboMenu.Add("useQCombo", new CheckBox("Use Q"));
            ComboMenu.Add("UseQAAcombo", new CheckBox("Use Q After AA", false));
            ComboMenu.AddLabel("W Settings:");
            ComboMenu.Add("useWCombo", new CheckBox("Use W"));
            ComboMenu.Add("UseWAAcombo", new CheckBox("Use W After AA", false));
            ComboMenu.Add("CCE", new CheckBox("Auto W on Enemy CC"));
            ComboMenu.Add("Wpred", new Slider("Select % Hitchance", 70, 0, 100));
            ComboMenu.AddLabel("Higher % ->Higher chance of spell landing on target but takes more time to get casted");
            ComboMenu.AddLabel("Lower % ->Faster casting but lower chance that the spell will land on your target. ");
            ComboMenu.AddLabel("R Settings:");
            ComboMenu.Add("useRCombo", new CheckBox("Use R [Hp%]"));
            ComboMenu.Add("Hp", new Slider("Use R Enemy Health %", 45, 0, 100));
            ComboMenu.Add("useRComboENEMIES", new CheckBox("Use R[Count Enemy]"));
            ComboMenu.Add("Rcount", new Slider("R When Enemies >= ", 2, 1, 5));
            ComboMenu.AddLabel("Use R Range Settigs For all Logic:");
            ComboMenu.Add("useRRange", new Slider("Use Ulty Max Range", 1800, 500, 2000));
            ComboMenu.Add("ForceR",new KeyBind("Force R On Target Selector", false, KeyBind.BindTypes.HoldActive, "T".ToCharArray()[0]));


            VolleyMenu = Menu.AddSubMenu("Volley Settings", "Volley");
            VolleyMenu.AddGroupLabel("Volley Settings:");
            VolleyMenu.Add("Volley.castDragon",
                new KeyBind("Send Volley to Dragon", false, KeyBind.BindTypes.HoldActive, 'U'));
            VolleyMenu.Add("Volley.castBaron",
                new KeyBind("Send Volley to Baron/Rift Herald", false, KeyBind.BindTypes.HoldActive, 'I'));
            VolleyMenu.Add("Volley.sep1", new Separator());
            VolleyMenu.Add("Volley.enable", new CheckBox("Auto send Volleys", false));
            VolleyMenu.Add("Volley.noMode", new CheckBox("Only when no modes are active"));
            VolleyMenu.Add("Volley.mana", new Slider("Minimum {0}% mana to auto send E", 40));
            VolleyMenu.Add("Volley.locationLabel", new Label("Send Volleys to:"));
            (VolleyMenu.Add("Volley.baron", new CheckBox("Baron / Rift Herald"))).OnValueChange += VolleyLocation;
            (VolleyMenu.Add("Volley.dragon", new CheckBox("Dragon"))).OnValueChange += VolleyLocation;

            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings:");
            HarassMenu.Add("useQHarass", new CheckBox("Use Q"));
            HarassMenu.Add("useWHarass", new CheckBox("Use W"));
            HarassMenu.Add("useWHarassMana", new Slider("W Mana > %", 70, 0, 100));
            HarassMenu.AddLabel("AutoHarass Settings:");
            HarassMenu.Add("autoWHarass", new CheckBox("Auto W for Harass", false));
            HarassMenu.Add("autoWHarassMana", new Slider("W Mana > %", 70, 0, 100));

            JungleLaneMenu = Menu.AddSubMenu("Lane Clear Settings", "FarmSettings");
            JungleLaneMenu.AddGroupLabel("Lane Clear Settings:");
            JungleLaneMenu.Add("useWFarm", new CheckBox("Use W"));
            JungleLaneMenu.Add("useWManalane", new Slider("W Mana > %", 70, 0, 100));
            JungleLaneMenu.AddLabel("Jungle Clear Settings:");
            // JungleLaneMenu.Add("useQJungle", new CheckBox("Use Q"));
            JungleLaneMenu.Add("useWJungle", new CheckBox("Use W"));
            JungleLaneMenu.Add("useWMana", new Slider("W Mana > %", 70, 0, 100));

            FleeMenu = Menu.AddSubMenu("Flee Settings", "FleeSettings");
            FleeMenu.Add("FleeQ", new CheckBox("Use W"));

            MiscMenu = Menu.AddSubMenu("Misc Settings", "MiscSettings");
            MiscMenu.AddGroupLabel("Gapcloser Settings:");
            MiscMenu.Add("gapcloser", new CheckBox("Auto W for Gapcloser"));
            MiscMenu.AddLabel("Interrupter Settings:");
            MiscMenu.Add("interrupter", new CheckBox("Enable Interrupter Using R"));
            MiscMenu.Add("interrupt.value", new ComboBox("Interrupter DangerLevel", 0, "High", "Medium", "Low"));
            MiscMenu.AddGroupLabel("Ks Settings:");
            MiscMenu.Add("UseWks", new CheckBox("Use W ks"));
            MiscMenu.Add("UseRks", new CheckBox("Use R ks"));
          
            AutoPotHealMenu = Menu.AddSubMenu("Potion & HeaL", "Potion & HeaL");
            AutoPotHealMenu.AddGroupLabel("Auto pot usage");
            AutoPotHealMenu.Add("potion", new CheckBox("Use potions"));
            AutoPotHealMenu.Add("potionminHP", new Slider("Minimum Health % to use potion", 40));
            AutoPotHealMenu.Add("potionMinMP", new Slider("Minimum Mana % to use potion", 20));
            AutoPotHealMenu.AddGroupLabel("AUto Heal Usage");
            AutoPotHealMenu.Add("UseHeal", new CheckBox("Use Heal"));
            AutoPotHealMenu.Add("useHealHP", new Slider("Minimum Health % to use Heal", 20));

            ItemMenu = Menu.AddSubMenu("Item Settings", "ItemMenuettings");
            ItemMenu.Add("useBOTRK", new CheckBox("Use BOTRK"));
            ItemMenu.Add("useBotrkMyHP", new Slider("My Health < ", 60, 1, 100));
            ItemMenu.Add("useBotrkEnemyHP", new Slider("Enemy Health < ", 60, 1, 100));
            ItemMenu.Add("useYoumu", new CheckBox("Use Youmu"));
            ItemMenu.AddLabel("QQs Settings");
            ItemMenu.Add("useQSS", new CheckBox("Use QSS"));
            ItemMenu.Add("Qssmode", new ComboBox(" ", 0, "Auto", "Combo"));
            ItemMenu.Add("Stun", new CheckBox("Stun", true));
            ItemMenu.Add("Blind", new CheckBox("Blind", true));
            ItemMenu.Add("Charm", new CheckBox("Charm", true));
            ItemMenu.Add("Suppression", new CheckBox("Suppression", true));
            ItemMenu.Add("Polymorph", new CheckBox("Polymorph", true));
            ItemMenu.Add("Fear", new CheckBox("Fear", true));
            ItemMenu.Add("Taunt", new CheckBox("Taunt", true));
            ItemMenu.Add("Silence", new CheckBox("Silence", false));
            ItemMenu.Add("QssDelay", new Slider("Use QSS Delay(ms)", 250, 0, 1000));


            SkinMenu = Menu.AddSubMenu("Skin Changer", "SkinChanger");
            SkinMenu.Add("checkSkin", new CheckBox("Use Skin Changer"));
            SkinMenu.Add("skin.Id", new Slider("Skin", 1, 0, 9));

            DrawMenu = Menu.AddSubMenu("Drawing Settings");
            DrawMenu.Add("drawRange", new CheckBox("Draw Q Range"));
            DrawMenu.Add("drawW", new CheckBox("Draw W Range"));
            DrawMenu.Add("drawR", new CheckBox("Draw R Range"));
            DrawMenu.AddLabel("Recall Tracker");
            DrawMenu.Add("draw.Recall", new CheckBox("Chat Print"));
          
            Game.OnTick += Game_OnTick;
            Game.OnUpdate += OnGameUpdate;
            Obj_AI_Base.OnBuffGain += OnBuffGain;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Drawing.OnDraw += Drawing_OnDraw;
            Orbwalker.OnPostAttack += OnAfterAttack;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            {
                if (DrawMenu["drawRange"].Cast<CheckBox>().CurrentValue)
                {
                    if (_q.IsReady()) new Circle {Color = Color.Aqua, Radius = _q.Range}.Draw(_Player.Position);
                    else if (_q.IsOnCooldown)
                        new Circle {Color = Color.Gray, Radius = _q.Range}.Draw(_Player.Position);
                }

                if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
                {
                    if (_w.IsReady()) new Circle {Color = Color.Aqua, Radius = _w.Range}.Draw(_Player.Position);
                    else if (_w.IsOnCooldown)
                        new Circle {Color = Color.Gray, Radius = _w.Range}.Draw(_Player.Position);
                }

                if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
                {
                    if (_r.IsReady()) new Circle {Color = Color.Aqua, Radius = _r.Range}.Draw(_Player.Position);
                    else if (_r.IsOnCooldown)
                        new Circle {Color = Color.Gray, Radius = _r.Range}.Draw(_Player.Position);
                }
            }
        }

        private static string FormatTime(double time)
        {
            var t = TimeSpan.FromSeconds(time);
            return string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }

        private static void Teleport_OnTeleport(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
        {
            if (sender.Team == _Player.Team || !DrawMenu["draw.Recall"].Cast<CheckBox>().CurrentValue) return;

            if (args.Status == TeleportStatus.Start)
            {
                Chat.Print("<font color='#ffffff'>[" + FormatTime(Game.Time) + "]</font> " + sender.BaseSkinName + " has <font color='#00ff66'>started</font> recall.");
            }

            if (args.Status == TeleportStatus.Abort)
            {
                Chat.Print("<font color='#ffffff'>[" + FormatTime(Game.Time) + "]</font> " + sender.BaseSkinName + " has <font color='#ff0000'>aborted</font> recall.");
            }

            if (args.Status == TeleportStatus.Finish)
            {
                Chat.Print("<font color='#ffffff'>[" + FormatTime(Game.Time) + "]</font> " + sender.BaseSkinName + " has <font color='#fdff00'>finished</font> recall.");
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (CheckSkin())
            {
                Player.SetSkinId(SkinId());
            }
        }

        public static int SkinId()
        {
            return SkinMenu["skin.Id"].Cast<Slider>().CurrentValue;
        }

        public static bool CheckSkin()
        {
            return SkinMenu["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

        public static void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (MiscMenu["gapcloser"].Cast<CheckBox>().CurrentValue && sender.IsEnemy &&
                e.End.Distance(_Player) <= 350)
            {
                _w.Cast(e.End);
            }
        }

        public static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs e)
        {
            var value = MiscMenu["interrupt.value"].Cast<ComboBox>().CurrentValue;
            var danger = value == 0
                ? DangerLevel.High
                : value == 1 ? DangerLevel.Medium : value == 2 ? DangerLevel.Low : DangerLevel.High;
            if (sender.IsEnemy
                && MiscMenu["interrupter"].Cast<CheckBox>().CurrentValue
                && sender.IsValidTarget(_r.Range - 800)
                && e.DangerLevel == danger)
            {
                _r.Cast(sender);
            }
        }

        public static void OnAfterAttack(AttackableUnit target, EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && target.IsValid)
            {
                if (target == null || !(target is AIHeroClient) || target.IsDead || target.IsInvulnerable ||
                    !target.IsEnemy || target.IsPhysicalImmune || target.IsZombie)
                    return;
                var enemy = target as AIHeroClient;
                if (enemy == null)
                    return;
                if (_q.IsReady() && ComboMenu["UseQAAcombo"].Cast<CheckBox>().CurrentValue)
                {
                    foreach (var a in Player.Instance.Buffs)
                        if (a.Name == "asheqcastready" && a.Count == 4)
                        {
                            _q.Cast();
                        }
                }
                if (_w.IsReady() && ComboMenu["UseWAAcombo"].Cast<CheckBox>().CurrentValue)
                {
                    _w.Cast(enemy);
                }
            }
        }

        private static
            void OnGameUpdate(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
                UseQ();
                ItemUsage();
                AUtoheal();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                WaveClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }
            Ks();
            Auto();
            AutoW();
            UseRTarget();
            AutoPot();
            AutoVolley();
        }

        private static void AUtoheal()
        {
            if (Heal != null && AutoPotHealMenu["UseHeal"].Cast<CheckBox>().CurrentValue && Heal.IsReady() &&
                HealthPercent <= AutoPotHealMenu["useHealHP"].Cast<Slider>().CurrentValue
                && _Player.CountEnemiesInRange(600) > 0 && Heal.IsReady())
            {
                Heal.Cast();
            }
        }

        private static
            void AutoPot()
        {
            if (AutoPotHealMenu["potion"].Cast<CheckBox>().CurrentValue && !Player.Instance.IsInShopRange() &&
                Player.Instance.HealthPercent <= AutoPotHealMenu["potionminHP"].Cast<Slider>().CurrentValue &&
                !(Player.Instance.HasBuff("RegenerationPotion") || Player.Instance.HasBuff("ItemCrystalFlaskJungle") ||
                  Player.Instance.HasBuff("ItemMiniRegenPotion") || Player.Instance.HasBuff("ItemCrystalFlask") ||
                  Player.Instance.HasBuff("ItemDarkCrystalFlask")))
            {
                if (Item.HasItem(HealthPotion.Id) && Item.CanUseItem(HealthPotion.Id))
                {
                    HealthPotion.Cast();
                    return;
                }
                if (Item.HasItem(TotalBiscuit.Id) && Item.CanUseItem(TotalBiscuit.Id))
                {
                    TotalBiscuit.Cast();
                    return;
                }
                if (Item.HasItem(RefillablePotion.Id) && Item.CanUseItem(RefillablePotion.Id))
                {
                    RefillablePotion.Cast();
                    return;
                }
                if (Item.HasItem(CorruptingPotion.Id) && Item.CanUseItem(CorruptingPotion.Id))
                {
                    CorruptingPotion.Cast();
                    return;
                }
            }
            if (Player.Instance.ManaPercent <= AutoPotHealMenu["potionMinMP"].Cast<Slider>().CurrentValue &&
                !(Player.Instance.HasBuff("RegenerationPotion") || Player.Instance.HasBuff("ItemMiniRegenPotion") ||
                  Player.Instance.HasBuff("ItemCrystalFlask") || Player.Instance.HasBuff("ItemDarkCrystalFlask")))
            {
                if (Item.HasItem(CorruptingPotion.Id) && Item.CanUseItem(CorruptingPotion.Id))
                {
                    CorruptingPotion.Cast();
                  }
            }
        }

        public static
            void ItemUsage()
        {
            var target = TargetSelector.GetTarget(550, DamageType.Physical); // 550 = Botrk.Range


            if (ItemMenu["useYoumu"].Cast<CheckBox>().CurrentValue && Youmuu.IsOwned() && Youmuu.IsReady())
            {
                if (ObjectManager.Player.CountEnemiesInRange(1500) == 1)
                {
                    Youmuu.Cast();
                }
            }
            if (target != null)
            {
                if (ItemMenu["useBOTRK"].Cast<CheckBox>().CurrentValue && Item.HasItem(Cutlass.Id) &&
                    Item.CanUseItem(Cutlass.Id) &&
                    Player.Instance.HealthPercent < ItemMenu["useBotrkMyHP"].Cast<Slider>().CurrentValue &&
                    target.HealthPercent < ItemMenu["useBotrkEnemyHP"].Cast<Slider>().CurrentValue)
                {
                    Item.UseItem(Cutlass.Id, target);
                }
                if (ItemMenu["useBOTRK"].Cast<CheckBox>().CurrentValue && Item.HasItem(Botrk.Id) &&
                    Item.CanUseItem(Botrk.Id) &&
                    Player.Instance.HealthPercent < ItemMenu["useBotrkMyHP"].Cast<Slider>().CurrentValue &&
                    target.HealthPercent < ItemMenu["useBotrkEnemyHP"].Cast<Slider>().CurrentValue)
                {
                    Botrk.Cast(target);
                }
            }
        }

        private static void OnBuffGain(Obj_AI_Base sender, Obj_AI_BaseBuffGainEventArgs args)
        {
            if (!sender.IsMe) return;
            var type = args.Buff.Type;

            if (ItemMenu["Qssmode"].Cast<ComboBox>().CurrentValue == 0)
            {
                if (type == BuffType.Taunt && ItemMenu["Taunt"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Stun && ItemMenu["Stun"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Snare && ItemMenu["Snare"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Polymorph && ItemMenu["Polymorph"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Blind && ItemMenu["Blind"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Flee && ItemMenu["Fear"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Charm && ItemMenu["Charm"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Suppression && ItemMenu["Suppression"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Silence && ItemMenu["Silence"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
            }
            if (ItemMenu["Qssmode"].Cast<ComboBox>().CurrentValue == 1 &&
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                if (type == BuffType.Taunt && ItemMenu["Taunt"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Stun && ItemMenu["Stun"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Snare && ItemMenu["Snare"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Polymorph && ItemMenu["Polymorph"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Blind && ItemMenu["Blind"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Flee && ItemMenu["Fear"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Charm && ItemMenu["Charm"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Suppression && ItemMenu["Suppression"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
                if (type == BuffType.Silence && ItemMenu["Silence"].Cast<CheckBox>().CurrentValue)
                {
                    DoQss();
                }
            }
        }

        private static void DoQss()
        {
            if (ItemMenu["useQSS"].Cast<CheckBox>().CurrentValue && Qss.IsOwned() && Qss.IsReady() &&
                ObjectManager.Player.CountEnemiesInRange(1800) > 0)
            {
                Core.DelayAction(() => Qss.Cast(), ItemMenu["QssDelay"].Cast<Slider>().CurrentValue);
            }
            if (Simitar.IsOwned() && Simitar.IsReady() && ObjectManager.Player.CountEnemiesInRange(1800) > 0)
            {
                Core.DelayAction(() => Simitar.Cast(), ItemMenu["QssDelay"].Cast<Slider>().CurrentValue);
            }
        }

        private static void AutoVolley()
        {
            if (VolleyMenu["Volley.castBaron"].Cast<KeyBind>().CurrentValue)
            {
                CastW(Baron);
            }
            if (VolleyMenu["Volley.castDragon"].Cast<KeyBind>().CurrentValue)
            {
                CastW(Dragon);
            }
        }

        public static void CastW(Vector2 location)
        {
            if (!_e.IsReady()) return;

            if (Player.Instance.Distance(location) <= _e.Range)
            {
                _e.Cast(location.To3DWorld());
            }
        }

        public static void Flee()
        {
            var targetW = TargetSelector.GetTarget(_w.Range, DamageType.Physical);
            var fleeQ = FleeMenu["FleeQ"].Cast<CheckBox>().CurrentValue;

            if (fleeQ && _w.IsReady() && targetW.IsValidTarget(_w.Range))
            {
                _w.Cast(targetW);
            }
        }

        public static void Auto()
        {
            var eonCc = ComboMenu["CCE"].Cast<CheckBox>().CurrentValue;
            if (eonCc)
            {
                foreach (var enemy in EntityManager.Heroes.Enemies)
                {
                    if (enemy.Distance(Player.Instance) < _w.Range &&
                        (enemy.HasBuffOfType(BuffType.Stun)
                         || enemy.HasBuffOfType(BuffType.Snare)
                         || enemy.HasBuffOfType(BuffType.Suppression)
                         || enemy.HasBuffOfType(BuffType.Fear)
                         || enemy.HasBuffOfType(BuffType.Knockup)))
                    {
                        _w.Cast(enemy);
                    }
                }
            }
        }

        public static void Ks()
        {
            var usewks = MiscMenu["UseWks"].Cast<CheckBox>().CurrentValue;
            var useRks = MiscMenu["UseRks"].Cast<CheckBox>().CurrentValue;
            foreach (
                var enemy in
                    EntityManager.Heroes.Enemies.Where(
                        e => e.Distance(_Player) <= _w.Range && e.IsValidTarget() && !e.IsInvulnerable))
            {
                if (usewks && _w.IsReady() &&
                    DmgLibrary.WDamage(enemy) >= enemy.Health && enemy.Distance(_Player) >= 650)
                {
                    var predW = _w.GetPrediction(enemy);
                    if (predW.HitChance == HitChance.High)
                    {
                        _w.Cast(predW.CastPosition);
                    }
                }
                if (useRks && _r.IsReady() &&
                    DmgLibrary.RDamage(enemy) >= enemy.Health && enemy.Distance(_Player) >= 900)
                {
                    var predR = _r.GetPrediction(enemy);
                    if (predR.HitChance == HitChance.Medium)
                    {
                        _r.Cast(predR.CastPosition);
                    }
                }
            }
        }

        public static void JungleClear()
        {
            var useW = JungleLaneMenu["useWJungle"].Cast<CheckBox>().CurrentValue;
          //  var useq = JungleLaneMenu["useQJungle"].Cast<CheckBox>().CurrentValue;
            var junglemana = JungleLaneMenu["useWMana"].Cast<Slider>().CurrentValue;

            if (Orbwalker.IsAutoAttacking) return;
            {
                if (useW && Player.Instance.ManaPercent > junglemana)
                {
                    var minions =
                        EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.Position, _w.Range)
                            .Where(t => !t.IsDead && t.IsValid && !t.IsInvulnerable);
                    if (minions.Count() > 0)
                    {
                        _w.Cast(minions.First());
                    }
                }
            }
        }

        public static
            void WaveClear()
        {
            var useW = JungleLaneMenu["useWFarm"].Cast<CheckBox>().CurrentValue;
            var junglemana = JungleLaneMenu["useWManalane"].Cast<Slider>().CurrentValue;


            if (Orbwalker.IsAutoAttacking) return;

            if (useW && Player.Instance.ManaPercent > junglemana)
            {
                var minions =
                    EntityManager.MinionsAndMonsters.EnemyMinions.Where(
                        t =>
                            t.IsEnemy && !t.IsDead && t.IsValid && !t.IsInvulnerable &&
                            t.IsInRange(Player.Instance.Position, _w.Range));
                foreach (var m in minions)
                {
                    if (
                        _w.GetPrediction(m)
                            .CollisionObjects.Where(t => t.IsEnemy && !t.IsDead && t.IsValid && !t.IsInvulnerable)
                            .Count() >= 0)
                    {
                        _w.Cast(m);
                        break;
                    }
                }
            }
        }

        public static void AutoW()
        {
            var targetW = TargetSelector.GetTarget(_w.Range, DamageType.Physical);
            if (HarassMenu["autoWHarass"].Cast<CheckBox>().CurrentValue &&
                _w.IsReady() && targetW.IsValidTarget(_w.Range) &&
                Player.Instance.ManaPercent > HarassMenu["autoWHarassMana"].Cast<Slider>().CurrentValue)
            {
                _w.Cast(targetW);
            }
        }

        public static
            void Harass()
        {
            var targetW = TargetSelector.GetTarget(_w.Range, DamageType.Physical);
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Physical);
            var wmana = HarassMenu["useWHarassMana"].Cast<Slider>().CurrentValue;
            var wharass = HarassMenu["useWHarass"].Cast<CheckBox>().CurrentValue;
            var useQharass = HarassMenu["useQHarass"].Cast<CheckBox>().CurrentValue;

            Orbwalker.ForcedTarget = null;

            if (Orbwalker.IsAutoAttacking) return;

            if (targetW != null)
            {
                if (wharass && _w.IsReady() &&
                    target.Distance(_Player) > _Player.AttackRange &&
                    targetW.IsValidTarget(_w.Range) && Player.Instance.ManaPercent > wmana)
                {
                    _w.Cast(targetW);
                }
            }

            if (target != null)
            {
                if (useQharass && _q.IsReady())
                {
                    if (target.Distance(_Player) <= Player.Instance.AttackRange)
                    {
                        _q.Cast();
                    }
                }
            }
        }

        private static void UseQ()
        {
            if (ComboMenu["useQCombo"].Cast<CheckBox>().CurrentValue && _q.IsReady())
            {
                if (Player.Instance.CountEnemiesInRange(600) >= 1)
                {
                    foreach (var a in Player.Instance.Buffs)
                        if (a.Name == "asheqcastready" && a.Count == 4)
                        {
                            _q.Cast();
                        }
                }
            }
        }
        
        private static
            void Combo()
        {
            var rCount = ComboMenu["Rcount"].Cast<Slider>().CurrentValue;
            var comboR = ComboMenu["useRComboENEMIES"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["useRcombo"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["useWCombo"].Cast<CheckBox>().CurrentValue;
            var hp = ComboMenu["Hp"].Cast<Slider>().CurrentValue;
            var wpred = ComboMenu["Wpred"].Cast<Slider>().CurrentValue;
            var targetR = TargetSelector.GetTarget(_r.Range, DamageType.Magical);
            var target = TargetSelector.GetTarget(_q.Range, DamageType.Magical);
            var distance = ComboMenu["useRRange"].Cast<Slider>().CurrentValue;

            if (target == null || !target.IsValidTarget()) return;

            if (_w.IsReady() && useW && target.IsValidTarget(1000))
            {
                var predW = _w.GetPrediction(target);
                if (predW.HitChancePercent >= wpred)
                {
                    _w.Cast(predW.CastPosition);
                }
            }
            if (_r.IsReady() && useR && targetR.Distance(_Player) <= distance &&
                _r.GetPrediction(targetR).HitChance >= HitChance.High && target.HealthPercent <= hp)
            {
                _r.Cast(_r.GetPrediction(targetR).CastPosition);
            }
            if (comboR && _Player.CountEnemiesInRange(1800) >= rCount && _r.IsReady()
                && targetR.Distance(_Player) <= distance && targetR != null &&
                _r.GetPrediction(targetR).HitChance >= HitChance.High)
            {
                _r.Cast(_r.GetPrediction(targetR).CastPosition);
            }
        }

        public static
            void UseRTarget()
        {
            var target = TargetSelector.GetTarget(_r.Range, DamageType.Magical);
            if (target != null &&
                (ComboMenu["ForceR"].Cast<KeyBind>().CurrentValue && _r.IsReady() && target.IsValid &&
                 !Player.HasBuff("AsheR"))) _r.Cast(target.Position);
        }
    }
}