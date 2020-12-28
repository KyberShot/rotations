using System.Linq;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Drawing;
using AimsharpWow.API; 

namespace AimsharpWow.Modules
{
    public class DestructionLock : Rotation
    {
      
        List<string> Racials = new List<string>
        {
            "Blood Fury","Berserking","Fireblood",
        };

        List<string> CovenantAbilities = new List<string>
        {
            "Decimating Bolt","Impending Catastrophe","Scouring Tithe","Soul Rot"
        };

        List<string> BloodlustEffects = new List<string>
        {
            "Bloodlust","Heroism","Time Warp","Primal Rage","Drums of Rage"
        };

        List<string> GeneralBuffs = new List<string>
        {

        };

        List<string> GeneralDebuffs = new List<string>
        {

        };

        List<string> SpellsList = new List<string>
        {
           "Havoc","Conflagrate","Immolate","Chaos Bolt","Soul Fire","Shadowburn","Incinerate","Cataclysm","Rain of Fire","Channel Demonfire","Summon Infernal","Dark Soul: Instability"

        };

        List<string> BuffsList = new List<string>
        {
            "Backdraft","Chaotic Inferno","Crashing Chaos","Dark Soul: Instability","Grimoire of Supremacy"
        };

        List<string> DebuffsList = new List<string>
        {
           "Havoc","Immolate","Eradication","Roaring Blaze"
        };

        List<string> TotemsList = new List<string>
        {
            "Infernal"
        };

        List<string> MacroCommands = new List<string>
        {
            "AOE","SaveCooldowns","SaveCovenant"
        };



        public override void LoadSettings()
        {
            Settings.Add(new Setting("General Settings"));
            Settings.Add(new Setting("Use Top Trinket:", false));
            Settings.Add(new Setting("Use Bottom Trinket:", false));
            Settings.Add(new Setting("Use DPS Potion:", false));
            Settings.Add(new Setting("Potion name:", "Potion of Phantom Fire"));
        }

        public override void Initialize()
        { 
            Aimsharp.PrintMessage("Kyber Destruction Warlock - v 1.0", Color.Blue);
            Aimsharp.PrintMessage("These macros can be used for manual control:", Color.Blue);
            Aimsharp.PrintMessage("Recommended talents: 3203012", Color.Blue);
            Aimsharp.PrintMessage("/xxxxx SaveCovenant", Color.Blue);
            Aimsharp.PrintMessage("--Toggles using Covenant abilities on/off.", Color.Blue);
            Aimsharp.PrintMessage(" ");
            Aimsharp.PrintMessage("/xxxxx SaveCooldowns", Color.Blue);
            Aimsharp.PrintMessage("--Toggles the use of big cooldowns on/off.", Color.Blue);
            Aimsharp.PrintMessage(" ");
            Aimsharp.PrintMessage("/xxxxx AOE", Color.Blue);
            Aimsharp.PrintMessage("--Toggles AOE mode on/off.", Color.Blue);
            Aimsharp.PrintMessage(" ");
            Aimsharp.PrintMessage("--Replace xxxxx with first 5 letters of your addon, lowercase.", Color.Blue);
            Aimsharp.PrintMessage("This rotation will use Havoc on your focus target, so please make a macro to set focus quickly.", Color.Blue);
            Aimsharp.PrintMessage("/focus [@mouseover,nodead,exists]; [@target,exists]", Color.Blue);

            Aimsharp.Latency = 50;
            Aimsharp.QuickDelay = 125;
            Aimsharp.SlowDelay = 250;

            foreach (string Spell in SpellsList)
            {
                Spellbook.Add(Spell);
            }

            foreach (string Spell in CovenantAbilities)
            {
                Spellbook.Add(Spell);
            }

            foreach (string Spell in Racials)
            {
                Spellbook.Add(Spell);
            }

            foreach (string Buff in GeneralBuffs)
            {
                Buffs.Add(Buff);
            }

            foreach (string Buff in BuffsList)
            {
                Buffs.Add(Buff);
            }

            foreach (string Buff in BloodlustEffects)
            {
                Buffs.Add(Buff);
            }

            foreach (string Debuff in DebuffsList)
            {
                Debuffs.Add(Debuff);
            }

            foreach (string Debuff in GeneralDebuffs)
            {
                Debuffs.Add(Debuff);
            }

            foreach (string Totem in TotemsList)
            {
                Totems.Add(Totem);
            }

            Items.Add(GetString("Potion name:"));

            Macros.Add("DPS Pot", "/use " + GetString("Potion name:"));
            Macros.Add("havoc focus", "/cast [@focus] Havoc");
            Macros.Add("cata cursor", "/cast [@cursor] Cataclysm");
            Macros.Add("rof cursor", "/cast [@cursor] Rain of Fire");
            Macros.Add("inf cursor", "/cast [@cursor] Summon Infernal");
            Macros.Add("TopTrink", "/use 13");
            Macros.Add("BotTrink", "/use 14");

            foreach (string MacroCommand in MacroCommands)
            {
                CustomCommands.Add(MacroCommand);
            }

            CustomFunctions.Add("GetCovenant", "local spell = 0 local i = 1 while true do local spellName, spellSub = GetSpellBookItemName(i, BOOKTYPE_SPELL) if not spellName then do break end end if spellName == 'Decimating Bolt' then spell = 4 elseif spellName == 'Soul Rot' then spell = 3 elseif spellName == 'Impending Catastrophe' then spell = 2 elseif spellName == 'Scouring Tithe' then spell = 1 end i = i + 1 end return spell");
            CustomFunctions.Add("GetLegendarySpellID", "local power = 0 for i=1,15,1 do local xcs = ItemLocation:CreateFromEquipmentSlot(i) if(C_Item.DoesItemExist(xcs)) then if(C_LegendaryCrafting.IsRuneforgeLegendary(xcs)) then local id = C_LegendaryCrafting.GetRuneforgeLegendaryComponentInfo(xcs)[\"powerID\"] power = C_LegendaryCrafting.GetRuneforgePowerInfo(id)[\"descriptionSpellID\"] end end end return power");
        }





        public override bool CombatTick()
        {
            bool Fighting = Aimsharp.Range("target") <= 40 && Aimsharp.TargetIsEnemy();
            bool Moving = Aimsharp.PlayerIsMoving();
            float Haste = Aimsharp.Haste() / 100f;
            int Range = Aimsharp.Range("target");
            string LastCast = Aimsharp.LastCast();
            bool IsChanneling = Aimsharp.IsChanneling("player");
            int EnemiesInMelee = Aimsharp.EnemiesInMelee();
            int EnemiesNearTarget = Aimsharp.EnemiesNearTarget();
            int GCDMAX = (int)(1500f / (Haste + 1f));
            int GCD = Aimsharp.GCD();
            int Latency = Aimsharp.Latency;
            string PotionName = GetString("Potion name:");


            //custom settings
            bool UsePotion = GetCheckBox("Use DPS Potion:");
            bool UseTopTrinket = GetCheckBox("Use Top Trinket:");
            bool UseBottomTrinket = GetCheckBox("Use Bottom Trinket:");
            bool AOE = Aimsharp.IsCustomCodeOn("AOE");
            bool SaveCooldowns = Aimsharp.IsCustomCodeOn("SaveCooldowns");
            bool SaveCovenant = Aimsharp.IsCustomCodeOn("SaveCovenant");

            //Target time to die
            int TargetHealth = Aimsharp.Health("target");
            int TargetMaxHP = Aimsharp.TargetMaxHP();
            int TargetCurrentHP = Aimsharp.TargetCurrentHP();
            int Time = Aimsharp.CombatTime();
            var TargetDamageTakenPerSecond = (TargetMaxHP - TargetCurrentHP) / (Math.Floor((double)Time/1000));
            int TargetTimeToDie = (int)Math.Ceiling(TargetCurrentHP / TargetDamageTakenPerSecond);

            //Covenants
            int CovenantID = Aimsharp.CustomFunction("GetCovenant");
            bool Kyrian = CovenantID == 1;
            bool Venthyr = CovenantID == 2;
            bool NightFae = CovenantID == 3;
            bool Necrolord = CovenantID == 4;

            //legendary effects class
            int LegendaryID = Aimsharp.CustomFunction("GetLegendarySpellID");
            bool LegendaryCindersoftheAzjAqir = LegendaryID == 337166;
            bool LegendaryClawofEndereth = LegendaryID == 337038;
            bool LegendaryEmbersoftheDiabolicRaiment = LegendaryID == 337272;
            bool LegendaryMadnessoftheAzjAqir = LegendaryID == 337169;
            bool LegendaryOdrShawloftheYmirjar = LegendaryID == 337163;
            bool LegendaryPillarsoftheDarkPortal = LegendaryID == 337065;
            bool LegendaryRelicofDemonicSynergy = LegendaryID == 337057;
            bool LegendaryWilfredsSigilofSuperiorSummoning = LegendaryID == 337020;

            //legendary effects general
            bool LegendaryEchoofEonar = LegendaryID == 338477;
            bool LegendaryJudgmentoftheArbiter = LegendaryID == 339344;
            bool LegendaryMawRattle = LegendaryID == 340197;
            bool LegendaryNorgannonsSagacity = LegendaryID == 339340;
            bool LegendarySephuzsProclamation = LegendaryID == 339348;
            

            //Conduits & Soulbinds
            List<int> ActiveConduits = new List<int>();
            //Conduits
            bool ConduitBroodingPool = ActiveConduits.Contains(340063);

            //Soulbinds
            bool SoulbindLeadByExample = ActiveConduits.Contains(342156);



            bool BloodlustUp = false;
            foreach (string BloodlustEffect in BloodlustEffects)
            {
                if (Aimsharp.HasBuff(BloodlustEffect))
                {
                    BloodlustUp = true;
                    break;
                }
            }

            //soulshards
            float SoulShard = Aimsharp.PlayerSecondaryPower() / 10f;
            
            //Talents
            bool TalentInfernoEnabled = Aimsharp.Talent(4, 1);
            bool TalentInternalCombustionEnabled = Aimsharp.Talent(2, 2);
            bool TalentFireAndBrimstoneEnabled = Aimsharp.Talent(4, 2);
            bool TalentGrimoireOfSupremacyEnabled = Aimsharp.Talent(6, 3);
            bool TalentCataclysmEnabled = Aimsharp.Talent(4, 3);
            bool TalentDarkSoulInstabilityEnabled = Aimsharp.Talent(7, 3);
            bool TalentFlashoverEnabled = Aimsharp.Talent(1, 1);
            bool TalentEradicationEnabled = Aimsharp.Talent(1, 2);
            bool TalentRoaringBlazeEnabled = Aimsharp.Talent(6, 1);
            bool TalentChannelDemonfireEnabled = Aimsharp.Talent(7, 2);
            bool TalentSoulFireEnabled = Aimsharp.Talent(1, 3);
            bool TalentShadowBurnEnabled = Aimsharp.Talent(2, 3);

            //Debuffs
            int DebuffEradicationRemains = Aimsharp.DebuffRemaining("Eradication") - GCD;
            bool DebuffEradicationUp = DebuffEradicationRemains > 0;
            int DebuffImmolateRemains = Aimsharp.DebuffRemaining("Immolate") - GCD;
            bool DebuffImmolateUp = DebuffImmolateRemains > 0;
            bool DebuffImmolateRefreshable = DebuffImmolateRemains < 5400;
            int DebuffHavocRemains = Aimsharp.DebuffRemaining("Havoc", "focus") - GCD;
            bool DebuffHavocUp = DebuffHavocRemains > 0;
            int DebuffRoaringBlazeRemains = Aimsharp.DebuffRemaining("Roaring Blaze") - GCD;
            bool DebuffRoaringBlazeUp = DebuffRoaringBlazeRemains > 0;
            

            //Buffs
            int BuffBackdraftRemains = Aimsharp.BuffRemaining("Backdraft") - GCD;
            bool BuffBackdraftUp = BuffBackdraftRemains > 0;
            int BuffChaoticInfernoRemains = Aimsharp.BuffRemaining("Chaotic Inferno") - GCD;
            bool BuffChaoticInfernoUp = BuffChaoticInfernoRemains > 0;
            int BuffCrashingChaosRemains = Aimsharp.BuffRemaining("Crashing Chaos") - GCD;
            bool BuffCrashingChaosUp = BuffCrashingChaosRemains > 0;
            int BuffDarkSoulInstabilityRemains = Aimsharp.BuffRemaining("Dark Soul: Instability") - GCD;
            bool BuffDarkSoulInstabilityUp = BuffDarkSoulInstabilityRemains > 0;
            int BuffGrimoireOfSupremacyStacks = Aimsharp.BuffStacks("Grimoire of Supremacy");

            //Cooldowns
            int CDHavocRemains = Aimsharp.SpellCooldown("Havoc") - GCD;
            bool CDHavocReady = CDHavocRemains <= 10;
            int CDConflagrateRemains = Aimsharp.SpellCooldown("Conflagrate") - GCD;
            bool CDConflagrateReady = CDConflagrateRemains <= 10;
            int CDCataclysmRemains = Aimsharp.SpellCooldown("Cataclysm") - GCD;
            bool CDCataclysmReady = CDCataclysmRemains <= 10;
            int CDSummonInfernalRemains = Aimsharp.SpellCooldown("Summon Infernal") - GCD;
            bool CDSummonInfernalReady = CDSummonInfernalRemains <= 10;
            int CDDarkSoulInstabilityRemains = Aimsharp.SpellCooldown("Dark Soul: Instability") - GCD;
            bool CDDarkSoulInstabilityReady = CDDarkSoulInstabilityRemains <= 10;
            int DpsPotionCdRemaining = Aimsharp.ItemCooldown("Potion of Phantom Fire");
            bool DpsPotionCdReady = DpsPotionCdRemaining <= 0;
            int CDSoulRotRemains = Aimsharp.SpellCooldown("Soul Rot") - GCD;
            bool CDSoulRotReady = CDSoulRotRemains <= 10;
            int CDDecimatingBoltRemains = Aimsharp.SpellCooldown("Decimating Bolt") - GCD;
            bool CDDecimatingBoltReady = CDDecimatingBoltRemains <= 10;
            int CDImpendingCatastropheRemains = Aimsharp.SpellCooldown("Impending Catastrophe") - GCD;
            bool CDImpendingCatastropheReady = CDImpendingCatastropheRemains <= 10;
            int CDScouringTitheRemains = Aimsharp.SpellCooldown("Scouring Tithe") - GCD;
            bool CDScouringTitheReady = CDScouringTitheRemains <= 10;
            int CDChannelDemonfireRemains = Aimsharp.SpellCooldown("Channel Demonfire") - GCD;
            bool CDChannelDemonfireReady = CDChannelDemonfireRemains <= 10;
            int CDSoulFireRemains = Aimsharp.SpellCooldown("Soul Fire") - GCD;
            bool CDSoulFireReady = CDSoulFireRemains <= 10;
            int CDShadowburnRemains = Aimsharp.SpellCooldown("Shadowburn") - GCD;
            bool CDShadowburnReady = CDShadowburnRemains <= 10;

            //cast times
            bool HavocActive = Aimsharp.DebuffRemaining("Havoc", "focus") - GCD > 0;
            float ChaosBoltCastTime = 3000f / (1f + Haste) * (BuffBackdraftUp ? .7f : 1f);
            float IncinerateCastTime = 2000f / (1f + Haste) * (BuffChaoticInfernoUp ? 0f : 1f);
            float DemonfireCastTime = 3000f / (1f + Haste);
            float SoulFireCastTime = 4500f / (1f + Haste);
            float DecimatingBoltCastTime = 2500f / (1f + Haste);
            float ScouringTitheCasttime = 2000f / (1f + Haste);

            //infernal active
            int InfernalRemaining = Aimsharp.TotemTimer();
            bool InfernalActive = InfernalRemaining > GCD;

            //Charges
            int ConflagrateCharges = Aimsharp.SpellCharges("Conflagrate");
            int ConflagrateMaxCharges = Aimsharp.MaxCharges("Conflagrate");
            int ConflagrateRechargeTime = Aimsharp.RechargeTime("Conflagrate");
            int ConflagrateFullRechargeTime = (int)(ConflagrateRechargeTime + (13000f) / (1f + Haste)) * (ConflagrateMaxCharges - ConflagrateCharges - 1);
            float ConflagrateChargesFractional_temp = ConflagrateCharges + (1-(ConflagrateRechargeTime) / ((13000f) / (1f + Haste)));
            float ConflagrateChargesFractional = ConflagrateChargesFractional_temp > Aimsharp.MaxCharges("Conflagrate") ? Aimsharp.MaxCharges("Conflagrate") : ConflagrateChargesFractional_temp;

            bool CastingImmolate = Aimsharp.CastingID("player") == 348;
            bool LineOfSighted = Aimsharp.LineOfSighted();

            if (!AOE)
            {
                EnemiesNearTarget = 1;
                EnemiesInMelee = EnemiesInMelee > 0 ? 1 : 0;
            }

            if (IsChanneling)
                return false;

            //actions=call_action_list,name=havoc,if=havoc_active&active_enemies>1&active_enemies<5-talent.inferno.enabled+(talent.inferno.enabled&talent.internal_combustion.enabled)
            if (HavocActive && EnemiesNearTarget > 1 && EnemiesNearTarget < 5 - (TalentInfernoEnabled ? 1 : 0) + (TalentInfernoEnabled && TalentInternalCombustionEnabled ? 1 : 0))
            {
                //actions.havoc=conflagrate,if=buff.backdraft.down&soul_shard>=1&soul_shard<=4
                if (Aimsharp.CanCast("Conflagrate")){
                if (!BuffBackdraftUp && SoulShard >= 1 && SoulShard <= 4){
                    Aimsharp.Cast("Conflagrate");
                    return true;}
                }

                //actions.havoc+=/soul_fire,if=cast_time<havoc_remains
                if (Aimsharp.CanCast("Soul Fire") && !Moving && Fighting && TalentSoulFireEnabled && CDSoulFireReady){
                if (SoulFireCastTime < DebuffHavocRemains)
                    Aimsharp.Cast("Soul Fire");
                    return true;
                }

                //actions.havoc+=/decimating_bolt,if=cast_time<havoc_remains&soulbind.lead_by_example.enabled
                if (Necrolord && Aimsharp.CanCast("Decimating Bolt") && Fighting && !SaveCovenant && CDDecimatingBoltReady){
                if (DecimatingBoltCastTime < DebuffHavocRemains && SoulbindLeadByExample){
                    Aimsharp.Cast("Decimating Bolt");
                    return true;}
                }

                //actions.havoc+=/scouring_tithe,if=cast_time<havoc_remains
                if (Kyrian && Aimsharp.CanCast("Scouring Tithe") && Fighting && !SaveCovenant && CDScouringTitheReady){
                if (ScouringTitheCasttime < DebuffHavocRemains){
                    Aimsharp.Cast("Scouring Tithe");
                    return true;}
                }

                //actions.havoc+=/immolate,if=talent.internal_combustion.enabled&remains<duration*0.5|!talent.internal_combustion.enabled&refreshable
                if (Aimsharp.CanCast("Immolate") && !Moving && !CastingImmolate){
                if (TalentInternalCombustionEnabled && DebuffImmolateRemains < 9000 || !TalentInternalCombustionEnabled && DebuffImmolateRefreshable){
                    Aimsharp.Cast("Immolate");
                    return true;}
                }

                //actions.havoc+=/chaos_bolt,if=cast_time<havoc_remains
                if (Aimsharp.CanCast("Chaos Bolt") && !Moving){
                if (ChaosBoltCastTime < DebuffHavocRemains){
                    Aimsharp.Cast("Chaos Bolt");
                    return true;}
                }

                //actions.havoc+=/shadowburn
                if (Aimsharp.CanCast("Shadowburn") && Fighting && TalentShadowBurnEnabled && CDShadowburnReady){
                    Aimsharp.Cast("Shadowburn");
                    return true;
                }

                //actions.havoc+=/incinerate,if=cast_time<havoc_remains
                if (Aimsharp.CanCast("Incinerate") && Fighting && !Moving){
                if (IncinerateCastTime < DebuffHavocRemains){
                    Aimsharp.Cast("Incinerate");
                    return true;}
                }
            }

                //actions+=/conflagrate,if=talent.roaring_blaze.enabled&debuff.roaring_blaze.remains<1.5
                if (Aimsharp.CanCast("Conflagrate")){
                if (TalentRoaringBlazeEnabled && DebuffHavocRemains < 1500){
                    Aimsharp.Cast("Conflagrate");
                    return true;}
                }

                //actions+=/cataclysm,if=!(pet.infernal.active&dot.immolate.remains+1>pet.infernal.remains)|spell_targets.cataclysm>1
                if (Aimsharp.CanCast("Cataclysm", "player") && !Moving && Fighting && CDCataclysmReady && TalentCataclysmEnabled){
                if (!(InfernalActive && DebuffImmolateRemains + 1000 > InfernalRemaining) || EnemiesNearTarget > 1){
                    Aimsharp.Cast("cata cursor");
                    return true;}
                }

                //actions+=/call_action_list,name=aoe,if=active_enemies>2
                if (EnemiesNearTarget > 2)
                {
                //actions.aoe=rain_of_fire,if=pet.infernal.active&(!cooldown.havoc.ready|active_enemies>3)
                if (Aimsharp.CanCast("Rain of Fire", "player") && Fighting){
                if (InfernalActive && (!CDHavocReady || EnemiesNearTarget > 3)){
                    Aimsharp.Cast("rof cursor");
                    return true;}
                }
                
                //actions.aoe+=/soul_rot
                if (NightFae && Aimsharp.CanCast("Soul Rot") && Fighting && !SaveCovenant && CDSoulRotReady){
                    Aimsharp.Cast("Soul Rot");
                    return true;
                }

                //actions.aoe+=/channel_demonfire,if=dot.immolate.remains>cast_time
                if (Aimsharp.CanCast("Channel Demonfire", "player") && !Moving && Fighting && TalentChannelDemonfireEnabled && CDChannelDemonfireReady){
                if (DebuffImmolateRemains > DemonfireCastTime){
                    Aimsharp.Cast("Channel Demonfire");
                    return true;}
                }

                //actions.aoe+=/immolate,cycle_targets=1,if=remains<5&(!talent.cataclysm.enabled|cooldown.cataclysm.remains>remains)
                if (Aimsharp.CanCast("Immolate") && !Moving && !CastingImmolate){
                if (DebuffImmolateRemains < 5000 && (!TalentCataclysmEnabled || CDCataclysmRemains > DebuffImmolateRemains)){
                    Aimsharp.Cast("Immolate");
                    return true;}
                }

                if(!SaveCooldowns)
                {
                //actions.cds+=/summon_infernal
                if (Aimsharp.CanCast("Summon Infernal", "player") && Fighting && CDSummonInfernalReady){
                    Aimsharp.Cast("inf cursor");
                    return true;
                }

                //actions.cds+=/dark_soul_instability
                if (Aimsharp.CanCast("Dark Soul: Instability", "player") && Fighting && CDDarkSoulInstabilityReady && TalentDarkSoulInstabilityEnabled){
                    Aimsharp.Cast("Dark Soul: Instability");
                    return true;
                }

                //actions.cds+=/potion,if=pet.infernal.active
                if (UsePotion && Aimsharp.CanUseItem(PotionName, false) && DpsPotionCdReady && InfernalActive){
                    Aimsharp.Cast("DPS Pot", true);
                    return true;
                }
                //actions.cds+=/berserking,if=pet.infernal.active, actions.cds+=/blood_fury,if=pet.infernal.active ,actions.cds+=/fireblood,if=pet.infernal.active
                if (InfernalActive)
                foreach (string Racial in Racials){
                if (Aimsharp.CanCast(Racial, "player") && Fighting){
                    Aimsharp.Cast(Racial, true);
                    return true;}
                } 

                //actions.cds+=/use_items,if=pet.infernal.active|target.time_to_die<20
                if (Aimsharp.CanUseTrinket(0) && UseTopTrinket && Fighting){
                if (InfernalActive || TargetTimeToDie <= 20){
                    Aimsharp.Cast("TopTrinket", true);
                    return true;}
                }
                
                //actions.cds+=/use_items,if=pet.infernal.active|target.time_to_die<20
                if (Aimsharp.CanUseTrinket(1) && UseBottomTrinket && Fighting){
                if (InfernalActive || TargetTimeToDie <= 20){
                    Aimsharp.Cast("BottomTrinket", true);
                    return true;}
                    }
                }

                //actions.aoe+=/havoc,cycle_targets=1,if=!(target=self.target)&active_enemies<4
                if (Aimsharp.CanCast("Havoc", "focus") && !LineOfSighted){
                if (EnemiesNearTarget < 4){
                    Aimsharp.Cast("havoc focus");
                    return true;}
                }

                //actions.aoe+=/rain_of_fire
                if (Aimsharp.CanCast("Rain of Fire", "player") && Fighting){
                    Aimsharp.Cast("rof cursor");
                    return true;
                }

                //actions.aoe+=/havoc,cycle_targets=1,if=!(target=self.target)
                if (Aimsharp.CanCast("Havoc", "focus") && !LineOfSighted){
                    Aimsharp.Cast("havoc focus");
                    return true;
                }

                //actions.aoe+=/decimating_bolt,if=(soulbind.lead_by_example.enabled|!talent.fire_and_brimstone.enabled)
                if (Necrolord && Aimsharp.CanCast("Decimating Bolt") && Fighting && !SaveCovenant && CDDecimatingBoltReady){
                if (SoulbindLeadByExample || !TalentFireAndBrimstoneEnabled){
                    Aimsharp.Cast("Decimating Bolt");
                    return true;}
                }

                //actions.aoe+=/incinerate,if=talent.fire_and_brimstone.enabled&buff.backdraft.up&soul_shard<5-0.2*active_enemies
                if (Aimsharp.CanCast("Incinerate") && Fighting && !Moving){
                if (TalentFireAndBrimstoneEnabled && BuffBackdraftUp && SoulShard < 5 - 0.2 * (EnemiesNearTarget)){
                    Aimsharp.Cast("Incinerate");
                    return true;}
                }

                //actions.aoe+=/soul_fire
                if (Aimsharp.CanCast("Soul Fire") && !Moving && Fighting && TalentSoulFireEnabled && CDSoulFireReady){
                    Aimsharp.Cast("Soul Fire");
                    return true;
                }

                //actions.aoe+=/conflagrate,if=buff.backdraft.down
                if (Aimsharp.CanCast("Conflagrate")){
                if (!BuffBackdraftUp){
                    Aimsharp.Cast("Conflagrate");
                    return true;}
                }

                //actions.aoe+=/shadowburn,if=target.health.pct<20
                if (Aimsharp.CanCast("Shadowburn") && Fighting && TalentShadowBurnEnabled && CDShadowburnReady){
                if (TargetHealth < 20){
                    Aimsharp.Cast("Shadowburn");
                    return true;}
                }

                //actions.aoe+=/scouring_tithe,if=!(talent.fire_and_brimstone.enabled|talent.inferno.enabled)
                if (Kyrian && Aimsharp.CanCast("Scouring Tithe") && Fighting && !SaveCovenant && CDScouringTitheReady){
                if (!(TalentFireAndBrimstoneEnabled || TalentInfernoEnabled)){
                    Aimsharp.Cast("Scouring Tithe");
                    return true;}
                }

                //actions.aoe+=/impending_catastrophe,if=!(talent.fire_and_brimstone.enabled|talent.inferno.enabled)
                if (Venthyr && Aimsharp.CanCast("Impending Catastrophe") && Fighting && !SaveCovenant && CDImpendingCatastropheReady){
                if (!(TalentFireAndBrimstoneEnabled || TalentInfernoEnabled)){
                    Aimsharp.Cast("Impending Catastrophe");
                    return true;}
                }

                //actions.aoe+=/incinerate
                if (Aimsharp.CanCast("Incinerate") && Fighting && !Moving){
                    Aimsharp.Cast("Incinerate");
                    return true;}
                }
                
                //actions+=/soul_fire,cycle_targets=1,if=refreshable&soul_shard<=4&(!talent.cataclysm.enabled|cooldown.cataclysm.remains>remains)
                if (Aimsharp.CanCast("Soul Fire") && Fighting && !Moving && TalentSoulFireEnabled && CDSoulFireReady){
                if (DebuffImmolateRefreshable && SoulShard <= 4 && (!TalentCataclysmEnabled || CDCataclysmRemains > DebuffImmolateRemains)){
                    Aimsharp.Cast("Soul Fire");
                    return true;}
                }

                //actions+=/immolate,cycle_targets=1,if=refreshable&(!talent.cataclysm.enabled|cooldown.cataclysm.remains>remains)
                if (Aimsharp.CanCast("Immolate") && !Moving && !CastingImmolate){
                if (DebuffImmolateRefreshable && (!TalentCataclysmEnabled || CDCataclysmRemains > DebuffImmolateRemains)){
                    Aimsharp.Cast("Immolate");
                    return true;}
                }

                //actions+=/immolate,if=talent.internal_combustion.enabled&action.chaos_bolt.in_flight&remains<duration*0.5
                if (Aimsharp.CanCast("Immolate") && !Moving && !CastingImmolate){
                if (DebuffImmolateRefreshable && (!TalentCataclysmEnabled || CDCataclysmRemains > DebuffImmolateRemains)){
                    Aimsharp.Cast("Immolate");
                    return true;}
                }

                if(!SaveCooldowns)
                {
                //actions.cds+=/summon_infernal
                if (Aimsharp.CanCast("Summon Infernal", "player") && Fighting && CDSummonInfernalReady){
                    Aimsharp.Cast("inf cursor");
                    return true;
                }

                //actions.cds+=/dark_soul_instability
                if (Aimsharp.CanCast("Dark Soul: Instability", "player") && Fighting && CDDarkSoulInstabilityReady && TalentDarkSoulInstabilityEnabled){
                    Aimsharp.Cast("Dark Soul: Instability");
                    return true;
                }

                //actions.cds+=/potion,if=pet.infernal.active
                if (UsePotion && Aimsharp.CanUseItem(PotionName, false) && DpsPotionCdReady && InfernalActive){
                    Aimsharp.Cast("DPS Pot", true);
                    return true;
                }

                if (InfernalActive)
                foreach (string Racial in Racials){
                if (Aimsharp.CanCast(Racial, "player") && Fighting){
                    Aimsharp.Cast(Racial, true);
                    return true;}
                }

                //actions.cds+=/use_items,if=pet.infernal.active|target.time_to_die<20
                if (Aimsharp.CanUseTrinket(0) && UseTopTrinket && Fighting){
                if (InfernalActive || TargetTimeToDie <= 20){
                    Aimsharp.Cast("TopTrinket", true);
                    return true;}
                }

                //actions.cds+=/use_items,if=pet.infernal.active|target.time_to_die<20
                if (Aimsharp.CanUseTrinket(1) && UseBottomTrinket && Fighting){
                if (InfernalActive || TargetTimeToDie <= 20){
                    Aimsharp.Cast("BottomTrinket", true);
                    return true;}
                    }
                }

                //actions+=/channel_demonfire
                if (Aimsharp.CanCast("Channel Demonfire", "player") && !Moving && Fighting && TalentChannelDemonfireEnabled && CDChannelDemonfireReady){
                Aimsharp.Cast("Channel Demonfire");
                return true;
                }

                //actions+=/scouring_tithe
                if (Kyrian && Aimsharp.CanCast("Scouring Tithe") && Fighting && !SaveCovenant && CDScouringTitheReady){
                    Aimsharp.Cast("Scouring Tithe");
                    return true;
                }

                //actions+=/decimating_bolt
                if (Necrolord && Aimsharp.CanCast("Decimating Bolt") && Fighting && !SaveCovenant && CDDecimatingBoltReady){
                    Aimsharp.Cast("Decimating Bolt");
                    return true;
                }

                //actions+=/havoc,cycle_targets=1,if=!(target=self.target)&(dot.immolate.remains>dot.immolate.duration*0.5|!talent.internal_combustion.enabled)
                if (Aimsharp.CanCast("Havoc", "focus") && !LineOfSighted){
                if ((DebuffImmolateRemains > 9000 || !TalentInternalCombustionEnabled)){
                    Aimsharp.Cast("havoc focus");
                    return true;}
                }
                
                //actions+=/impending_catastrophe
                if (Venthyr && Aimsharp.CanCast("Impending Catastrophe") && Fighting && !SaveCovenant && CDImpendingCatastropheReady){
                    Aimsharp.Cast("Impending Catastrophe");
                    return true;
                }

                //actions+=/soul_rot
                if (NightFae && Aimsharp.CanCast("Soul Rot") && Fighting && !SaveCovenant && CDSoulRotReady){
                    Aimsharp.Cast("Soul Rot");
                    return true;
                }

                //actions+=/havoc,if=runeforge.odr_shawl_of_the_ymirjar.equipped
                if (Aimsharp.CanCast("Havoc") && Fighting){
                if (EnemiesNearTarget == 1 && LegendaryOdrShawloftheYmirjar){
                    Aimsharp.Cast("havoc");
                    return true;}
                }

                //actions+=/variable,name=pool_soul_shards,value=active_enemies>1&cooldown.havoc.remains<=10|cooldown.summon_infernal.remains<=15&talent.dark_soul_instability.enabled&cooldown.dark_soul_instability.remains<=15|talent.dark_soul_instability.enabled&cooldown.dark_soul_instability.remains<=15&(cooldown.summon_infernal.remains>target.time_to_die|cooldown.summon_infernal.remains+cooldown.summon_infernal.duration>target.time_to_die)
                bool PoolSoulShards = !SaveCooldowns && ( EnemiesNearTarget > 1 && CDHavocRemains <= 10000 || CDSummonInfernalRemains <= 15000 && (TalentDarkSoulInstabilityEnabled && CDDarkSoulInstabilityRemains <= 15000) || TalentDarkSoulInstabilityEnabled && CDDarkSoulInstabilityRemains <= 15000 && (CDSummonInfernalRemains > TargetTimeToDie || CDSummonInfernalRemains + 135000 > TargetTimeToDie) );

                //actions+=/conflagrate,if=buff.backdraft.down&soul_shard>=1.5-0.3*talent.flashover.enabled&!variable.pool_soul_shards
                if (Aimsharp.CanCast("Conflagrate")){
                if (!BuffBackdraftUp && SoulShard >= 1.5 * (TalentFlashoverEnabled ? 1 : 0) && !PoolSoulShards){
                    Aimsharp.Cast("Conflagrate");
                    return true;}
                }

                //actions+=/chaos_bolt,if=buff.dark_soul_instability.up
                if (Aimsharp.CanCast("Chaos Bolt") && !Moving){
                if (BuffDarkSoulInstabilityUp){
                    Aimsharp.Cast("Chaos Bolt");
                    return true;}
                }

                //actions+=/chaos_bolt,if=buff.backdraft.up&!variable.pool_soul_shards&!talent.eradication.enabled
                if (Aimsharp.CanCast("Chaos Bolt") && !Moving){
                if (BuffBackdraftUp && !PoolSoulShards && !TalentEradicationEnabled){
                    Aimsharp.Cast("Chaos Bolt");
                    return true;}
                }

                //actions+=/chaos_bolt,if=!variable.pool_soul_shards&talent.eradication.enabled&(debuff.eradication.remains<cast_time|buff.backdraft.up)
                if (Aimsharp.CanCast("Chaos Bolt") && !Moving){
                if (!PoolSoulShards && TalentEradicationEnabled && (DebuffEradicationRemains < ChaosBoltCastTime || BuffBackdraftUp)){
                    Aimsharp.Cast("Chaos Bolt");
                    return true;}
                }

                //actions+=/shadowburn,if=!variable.pool_soul_shards|soul_shard>=4.5
                if (Aimsharp.CanCast("Shadowburn") && Fighting && TalentShadowBurnEnabled && CDShadowburnReady){
                if (!PoolSoulShards || SoulShard >= 4.5){
                    Aimsharp.Cast("Shadowburn");
                    return true;}
                }

                //actions+=/chaos_bolt,if=(soul_shard>=4.5-0.2*active_enemies)
                if (Aimsharp.CanCast("Chaos Bolt") && !Moving){
                if (SoulShard >= 4.5 - 0.2* (EnemiesNearTarget)){
                    Aimsharp.Cast("Chaos Bolt");
                    return true;}
                }

                //actions+=/conflagrate,if=charges>1
                if (Aimsharp.CanCast("Conflagrate")){
                if (ConflagrateCharges > 1){
                    Aimsharp.Cast("Conflagrate");
                    return true;}
                }

                //actions+=/incinerate
                if (Aimsharp.CanCast("Incinerate") && Fighting && !Moving){
                    Aimsharp.Cast("Incinerate");
                    return true;
                }

            return false;
            }

        public override bool OutOfCombatTick()
        {
            return false;
        }

    }
}
