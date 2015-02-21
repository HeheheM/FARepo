﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace FuckingAwesomeRiven
{
    class Antispells
    {
        public static void init()
        {
            var mainMenu = MenuHandler.Config.AddSubMenu(new Menu("Anti GapCloser", "Anti GapCloser"));
            var spellMenu = mainMenu.AddSubMenu(new Menu("Enabled Spells", "Enabled SpellsAnti GapCloser"));
            mainMenu.AddItem(new MenuItem("EnabledGC", "Enabled").SetValue(false));

            var mainMenuinterrupter = MenuHandler.Config.AddSubMenu(new Menu("Interrupter", "Interrupter"));
            mainMenuinterrupter.AddItem(new MenuItem("EnabledInterrupter", "Enabled").SetValue(false));
            mainMenuinterrupter.AddItem(new MenuItem("minChannel", "Minimum Channel Priority").SetValue(new StringList(new [] {"HIGH", "MEDIUM", "LOW"})));
            

            foreach (var champ in ObjectManager.Get<Obj_AI_Hero>().Where(a => a.IsEnemy))
            {
                var champmenu = spellMenu.AddSubMenu(new Menu(champ.ChampionName, champ.ChampionName + "GC"));
                foreach (var gcSpell in AntiGapcloser.Spells)
                {
                    if (gcSpell.ChampionName == champ.ChampionName)
                    {
                        champmenu.AddItem(
                            new MenuItem(gcSpell.SpellName, gcSpell.SpellName + "- " + gcSpell.Slot).SetValue(true));
                    }
                }
            }

            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (MenuHandler.Config.Item(gapcloser.Sender.LastCastedSpellName()) == null)
                return;
            if (!MenuHandler.Config.Item(gapcloser.Sender.LastCastedSpellName()).GetValue<bool>() || !gapcloser.Sender.IsValidTarget()) return;
            if (gapcloser.Sender.Distance(ObjectManager.Player) < SpellHandler.WRange)
            {
                Queuer.add("W");
                return;
            }
            if (gapcloser.Sender.Distance(ObjectManager.Player) < SpellHandler.QRange && CheckHandler.QCount == 2)
            {
                Queuer.add("Q");
            }
        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!MenuHandler.Config.Item("EnabledInterrupter").GetValue<bool>() || !sender.IsValidTarget()) return;
            Interrupter2.DangerLevel a;
            switch (MenuHandler.Config.Item("minChannel").GetValue<StringList>().SelectedValue)
            {
                case "HIGH":
                    a = Interrupter2.DangerLevel.High;
                    break;
                case "MEDIUM":
                    a = Interrupter2.DangerLevel.Medium;
                    break;
                default:
                    a = Interrupter2.DangerLevel.Low;
                    break;
            }

            if (args.DangerLevel == Interrupter2.DangerLevel.High ||
                args.DangerLevel == Interrupter2.DangerLevel.Medium && a != Interrupter2.DangerLevel.High ||
                args.DangerLevel == Interrupter2.DangerLevel.Medium && a != Interrupter2.DangerLevel.Medium &&
                a != Interrupter2.DangerLevel.High)
            {
                if (sender.Distance(ObjectManager.Player) < SpellHandler.WRange)
                {
                    Queuer.add("W");
                    return;
                }
                if (sender.Distance(ObjectManager.Player) < 250 + 325)
                {
                    Queuer.add("E", sender.Position);
                    Queuer.add("W");
                    return;
                }
                if (sender.Distance(ObjectManager.Player) < SpellHandler.QRange && CheckHandler.QCount == 2)
                {
                    Queuer.add("Q");
                }
            }
        }
    }
}
