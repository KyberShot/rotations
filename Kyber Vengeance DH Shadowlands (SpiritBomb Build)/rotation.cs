using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

using AimsharpWow.API;

namespace AimsharpWow.Modules
{
    public class VengeanceShadowlands : Rotation
    {
        //Spells
        List<string> VengeanceSpells = new List<string>{
            "Demon Spikes","Fiery Brand","Immolation Aura","Fracture","Spirit Bomb","Soul Cleave","Sigil of Flame","Throw Glaive","Metamorphosis",
            "Fel Devastation","Shear", "Infernal Strike","Disrupt","Bulk Extraction","Felblade","Soul Barrier"
        };

        List<string> VengeanceBuffs = new List<string>{
            "Metamorphosis","Lifeblood","Demon Spikes","Soul Fragments","Fel Bombardment","Immolation Aura"
        };

        List<string> VengeanceDebuffs = new List<string>{
            "Razor Coral","Conductive Ink","Dark Slash","Spirit Bomb","Imprison","Sinful Brand","Fiery Brand"
        };

        List<string> BloodlustEffects = new List<string>
        {
            "Bloodlust","Heroism","Time Warp","Primal Rage","Drums of Rage"
        };

        List<string> CovenantAbilities = new List<string>
        {
            "Sinful Brand","The Hunt","Elysian Decree"
        };


        public override void LoadSettings()
        {
            Settings.Add(new Setting("General Settings"));
            Settings.Add(new Setting("Use Top Trinket:", false));
            Settings.Add(new Setting("Use Bottom Trinket:", false));
            Settings.Add(new Setting("Use DPS Potion:", false));
            List<string> PotionList = new List<string>(new string[] { "Potion of Spectral Agility", "Potion of Phantom Fire", "Potion of Empowered Exorcisms"});
            Settings.Add(new Setting("Potion name:", PotionList, "Potion of Spectral Agility"));
        }


        public override void Initialize()
        {
            Aimsharp.PrintMessage("Kyber Vengenace DH Shadowlands - v 2.3", Color.Blue);
            Aimsharp.PrintMessage("Recommended talents: 1233221", Color.Blue);
            Aimsharp.PrintMessage("These macros can be used for manual control:", Color.Blue);
            Aimsharp.PrintMessage("/xxxxx SaveMeta", Color.Blue);
            Aimsharp.PrintMessage("--Toggles the use of Metamorphosis on/off.", Color.Blue);
            Aimsharp.PrintMessage(" ");
            Aimsharp.PrintMessage("/xxxxx SaveCovenant", Color.Blue);
            Aimsharp.PrintMessage("--Toggles the use of Covenant abilities on/off.", Color.Blue);
            Aimsharp.PrintMessage(" ");
            Aimsharp.PrintMessage("/xxxxx AOE", Color.Blue);
            Aimsharp.PrintMessage("--Toggles AOE mode on/off.", Color.Blue);
            Aimsharp.PrintMessage(" ");
            Aimsharp.PrintMessage("/xxxxx Autodisrupt", Color.Blue);
            Aimsharp.PrintMessage("--Toggles Auto interrupt mode on/off.", Color.Blue);
            Aimsharp.PrintMessage(" ");
            Aimsharp.PrintMessage("/xxxxx Autoinfernal", Color.Blue);
            Aimsharp.PrintMessage("--Toggles Auto infernal Strike mode on/off.", Color.Blue);
            Aimsharp.PrintMessage("will use 1 charge for added dps and save 1 charge for movement.", Color.Purple);
            Aimsharp.PrintMessage("--Replace xxxxx with first 5 letters of your addon, lowercase.", Color.Blue);

            Aimsharp.Latency = 50;
            Aimsharp.QuickDelay = 125;
            Aimsharp.SlowDelay = 250;

            //Main Skills
            foreach(string skill in VengeanceSpells){
                Spellbook.Add(skill);
            }

            //Covenant
            foreach (string Spell in CovenantAbilities)
            {
                Spellbook.Add(Spell);
            }

            //Buffs
            foreach(string buff in VengeanceBuffs){
                Buffs.Add(buff);
            }

            //Debuffs
            foreach(string debuff in VengeanceDebuffs){
                Debuffs.Add(debuff);
            }


            foreach (string Buff in BloodlustEffects)
            {
                Buffs.Add(Buff);
            }


            Items.Add(GetString("Potion name:"));

            Macros.Add("DPS Pot", "/use " + GetString("Potion name:"));
            Macros.Add("TopTrinket", "/use 13");
            Macros.Add("BottomTrinket", "/use 14");

            Macros.Add("sigil self", "/cast [@player] Sigil of Flame");
            Macros.Add("infernal self","/cast [@player] Infernal Strike");
            Macros.Add("Elysian self","/cast [@player] Elysian Decree");

            CustomCommands.Add("SaveMeta");
            CustomCommands.Add("AOE");
            CustomCommands.Add("Autoinfernal");
            CustomCommands.Add("Autodisrupt");
            CustomCommands.Add("SaveCovenant");

            CustomFunctions.Add("GetCovenant", "local spell = 0 local i = 1 while true do local spellName, spellSub = GetSpellBookItemName(i, BOOKTYPE_SPELL) if not spellName then do break end end if spellName == 'Fodder to the Flame' then spell = 4 elseif spellName == 'The Hunt' then spell = 3 elseif spellName == 'Sinful Brand' then spell = 2 elseif spellName == 'Elysian Decree' then spell = 1 end i = i + 1 end return spell");
            CustomFunctions.Add("GetLegendarySpellID", "local power = 0 for i=1,15,1 do local xcs = ItemLocation:CreateFromEquipmentSlot(i) if(C_Item.DoesItemExist(xcs)) then if(C_LegendaryCrafting.IsRuneforgeLegendary(xcs)) then local id = C_LegendaryCrafting.GetRuneforgeLegendaryComponentInfo(xcs)[\"powerID\"] power = C_LegendaryCrafting.GetRuneforgePowerInfo(id)[\"descriptionSpellID\"] end end end return power");

        }

        
        public override bool CombatTick()
        {
            int GCD = Aimsharp.GCD();
            float Haste = Aimsharp.Haste() / 100f;
            int GCDMAX = (int)(1500f / (Haste + 1f));
            int Latency = Aimsharp.Latency;
            bool Moving = Aimsharp.PlayerIsMoving();
            bool Fighting = Aimsharp.Range("target") <= 8 && Aimsharp.TargetIsEnemy();
            bool Pulling = Aimsharp.Range("target") <= 30 && Aimsharp.TargetIsEnemy();//used for pulling mobs with throw glaive and NightFae Covenant
            bool SigilDrop = Aimsharp.Range("target") <= 4 && Aimsharp.TargetIsEnemy();//only used for sigils and kyrian covenant
            bool TargetIsEnemy = Aimsharp.TargetIsEnemy();
            bool IsChanneling = Aimsharp.IsChanneling("player");
            int EnemiesInMelee = Aimsharp.EnemiesInMelee();
            int RangeToTarget = Aimsharp.Range("target");
            int selfhealth = Aimsharp.Health("player");
            bool TargetIsBoss = Aimsharp.TargetIsBoss();
            int EnemiesNearTarget = Aimsharp.EnemiesNearTarget();
            string PotionName = GetString("Potion name:");

            //custom settings
            bool UsePotion = GetCheckBox("Use DPS Potion:");
            bool UseTopTrinket = GetCheckBox("Use Top Trinket:");
            bool UseBottomTrinket = GetCheckBox("Use Bottom Trinket:");
            bool AOE = Aimsharp.IsCustomCodeOn("AOE");
            bool SaveMeta = Aimsharp.IsCustomCodeOn("SaveMeta");
            bool SaveCovenant = Aimsharp.IsCustomCodeOn("SaveCovenant");
            bool Autodisrupt = Aimsharp.IsCustomCodeOn("Autodisrupt");
            bool Autoinfernal = Aimsharp.IsCustomCodeOn("Autoinfernal");

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

            //legendary effects 
            int LegendaryID = Aimsharp.CustomFunction("GetLegendarySpellID");
            bool LegendaryCollectiveAnguish = LegendaryID == 337504;
            bool LegendaryDarkestHour = LegendaryID == 337539;
            bool LegendaryDarkglareBoon = LegendaryID == 337534;
            bool LegendaryEchoofEonar = LegendaryID == 338477;
            bool LegendaryFelBombardment = LegendaryID == 337775;
            bool LegendaryFelFlameFortification = LegendaryID == 337545;
            bool LegendaryFierySoul = LegendaryID == 337547;
            bool LegendaryJudgmentoftheArbiter = LegendaryID == 339344;
            bool LegendaryMawRattle = LegendaryID == 340197;
            bool LegendaryNorgannonsSagacity = LegendaryID == 339340;
            bool LegendarySephuzsProclamation = LegendaryID == 339348;
            bool LegendaryRazelikhsDefilement = LegendaryID == 337544;
            bool LegendarySpiritoftheDarknessFlame = LegendaryID == 337541;

            //Conduits & Soulbinds
            List<int> ActiveConduits = new List<int>();
            //Conduits
            bool ConduitBroodingPool = ActiveConduits.Contains(340063);

            //Soulbinds
            bool SoulbindAscendantPhial = ActiveConduits.Contains(329776);



            bool BloodlustUp = false;
            foreach (string BloodlustEffect in BloodlustEffects)
            {
                if (Aimsharp.HasBuff(BloodlustEffect))
                {
                    BloodlustUp = true;
                    break;
                }
            }

            //player power
            int Fury = Aimsharp.Power("player");
            int FuryDefecit = Aimsharp.PlayerMaxPower() - Fury;

            //Talents
            bool TalentFracture = Aimsharp.Talent(4, 3);
            bool SpiritBombEnabled = Aimsharp.Talent(3, 3);
            bool TalentBulkExtraction = Aimsharp.Talent(7, 3);
            bool TalentFelblade = Aimsharp.Talent(1, 3);
            bool TalentSoulBarrier = Aimsharp.Talent(6, 3);
            bool TalentAgonizingFlames = Aimsharp.Talent(1, 2);
            bool TalentBurningAlive = Aimsharp.Talent(2, 3);
            bool TalentCharredFlesh = Aimsharp.Talent(3, 2);
            bool TalentAbyssalStrike = Aimsharp.Talent(1, 1);
            bool TalentFallout = Aimsharp.Talent(2, 2);

            //CD's 
            int SoulCleaveCDRemaining = Aimsharp.SpellCooldown("Soul Cleave") - GCD;
            bool SoulCleaveCDReady = SoulCleaveCDRemaining <= 10;
            int SoulCleaveCharges = Aimsharp.SpellCharges("Soul Cleave");
            int FelDevastationCDRemaining = Aimsharp.SpellCooldown("Fel Devastation") - GCD;
            bool FelDevastationCDReady = FelDevastationCDRemaining <= 10;
            int FelbladeCDRemaining = Aimsharp.SpellCooldown("Felblade") - GCD;
            bool FelbladeCDReady = FelbladeCDRemaining <= 10;
            int SoulBarrierCDRemaining = Aimsharp.SpellCooldown("Soul Barrier") - GCD;
            bool SoulBarrierCDReady = SoulBarrierCDRemaining <= 10;
            int FractureCDRemaining = Aimsharp.SpellCooldown("Fracture") - GCD;
            bool FractureCDReady = FractureCDRemaining <= 10;
            int FractureCharges = Aimsharp.SpellCharges("Fracture");
            int ThrowGlavieCDRemaining = Aimsharp.SpellCooldown("Throw Glaive") - GCD;
            bool ThrowGlavieCDReady = ThrowGlavieCDRemaining <= 10;
            int ImmolationAuraCDRemaining = Aimsharp.SpellCooldown("Immolation Aura") - GCD;
            bool ImmolationAuraCDReady = ImmolationAuraCDRemaining <= 10;
            int FieryBrandCDRemaining = Aimsharp.SpellCooldown("Fiery Brand") - GCD;
            bool FieryBrandCDReady = FieryBrandCDRemaining <= 10;
            int SigilCDRemaining = Aimsharp.SpellCooldown("Sigil of Flame") - GCD;
            bool SigilCDReady = SigilCDRemaining <= 10;
            int MetaCDRemaining = Aimsharp.SpellCooldown("Metamorphosis");
            bool MetaCDReady = MetaCDRemaining <= 10;
            int InfernalCharges = Aimsharp.SpellCharges("Infernal Strike");
            int DemonSpikeCharges = Aimsharp.SpellCharges("Demon Spikes");
            int DpsPotionCdRemaining = Aimsharp.ItemCooldown("Potion of Phantom Fire");
            bool DpsPotionCdReady = DpsPotionCdRemaining <= 0;

            //Buffs
            bool SinfulDebuffUp = Aimsharp.HasDebuff("Sinful Brand");
            bool BuffSoulFragmentUp = Aimsharp.HasBuff("Soul Fragments");
            int SoulFragStacks = Aimsharp.BuffStacks("Soul Fragments");
            int MetaRemains = Aimsharp.BuffRemaining("Metamorphosis") - GCD;
            bool MetaUp = Aimsharp.HasBuff("Metamorphosis");
            bool PoolingForMeta = MetaCDRemaining < 6000 && FuryDefecit > 30;
            bool BuffFelBombardmentUp = Aimsharp.HasBuff("Fel Bombardment");
            int FelBombardmentStacks = Aimsharp.BuffStacks("Fel Bombardment");
            bool BuffImmolationAuraUp = Aimsharp.HasBuff("Immolation Aura");
            bool DemonSpikesUp = Aimsharp.HasBuff("Demon Spikes");

            //Debuffs
            bool CoralDebuffUp = Aimsharp.HasDebuff("Razor Coral");
            bool InkUp = Aimsharp.HasDebuff("Conductive Ink");
            bool DebuffFieryBrandUp = Aimsharp.HasDebuff("Fiery Brand");
            
	    //HOA Interupts IDs
            bool Casting325700 = Aimsharp.CastingID("focus") == 325700; // Collect Souls
            bool Casting326607 = Aimsharp.CastingID("focus") == 326607; // Turn To Stone 
            bool Casting323552 = Aimsharp.CastingID("focus") == 323442; // Volley Of Power
            bool Casting325876 = Aimsharp.CastingID("focus") == 325876; // Curse Of Obliteration
	    
	    //TOP Interupts IDs
            bool Casting330784 = Aimsharp.CastingID("focus") == 330784; // Necrotic Bolt
            bool Casting330562 = Aimsharp.CastingID("focus") == 330562; // Demoralizing Shout 
            bool Casting341977 = Aimsharp.CastingID("focus") == 341977; // Meat Shield
            bool Casting341969 = Aimsharp.CastingID("focus") == 341969; // Withering Discharge
            bool Casting370875 = Aimsharp.CastingID("focus") == 330875; // Spirit Frost
            bool Casting330868 = Aimsharp.CastingID("focus") == 330868; // Necrotic Bolt Volley 
            bool Casting342675 = Aimsharp.CastingID("focus") == 341977; // Bone Spear

            //Interrupt
            bool CanInterruptEnemy = Aimsharp.IsInterruptable();
			bool EnemyIsCasting = Aimsharp.IsChanneling("target") || Aimsharp.CastingRemaining("target") > 0;
			int EnemyCastRemaining = Aimsharp.CastingRemaining("target");

            //Spirit Bomb
            int DebuffSpiritBombRemains = Aimsharp.BuffRemaining("Spirit Bomb") - GCD;
            bool DebuffSpiritBombUp = DebuffSpiritBombRemains > 0;

            if (Autodisrupt){
            if (Aimsharp.CanCast("Disrupt", "target") && CanInterruptEnemy && EnemyIsCasting){
                Aimsharp.Cast("Disrupt");
                return true;}
            }

            if (!AOE)
            {
                EnemiesInMelee = EnemiesInMelee > 0 ? 1 : 0;
            }

            if (IsChanneling)
                return false;

            if (Aimsharp.HasDebuff("Imprison"))
                return false;

            if (UsePotion && Aimsharp.CanUseItem(PotionName, false) && DpsPotionCdReady){
                Aimsharp.Cast("DPS Pot", true);
                return true;
            }


            if (Aimsharp.CanUseTrinket(0) && UseTopTrinket && Fighting){
                Aimsharp.Cast("TopTrinket", true);
                return true;
            }

            if (Aimsharp.CanUseTrinket(1) && UseBottomTrinket && Fighting){
                Aimsharp.Cast("BottomTrinket", true);
                return true;
            }

            if (Aimsharp.CanCast("Spirit Bomb", "player") && Fighting){
            if (SoulFragStacks >=4 || EnemiesInMelee >=3 && selfhealth <=50){
                Aimsharp.Cast("Spirit Bomb");
                return true;}
            }

            //Defensives
            if (Aimsharp.CanCast("Demon Spikes", "player") && Fighting){
            if (DemonSpikeCharges >= 2 && !DemonSpikesUp && !MetaUp && !DebuffFieryBrandUp){
                Aimsharp.Cast("Demon Spikes");
                return true;}
            }

            if (Aimsharp.CanCast("Demon Spikes", "player") && Fighting){
            if (!DemonSpikesUp && selfhealth <= 50){
                Aimsharp.Cast("Demon Spikes");
                return true;}
            }

            if (Aimsharp.CanCast("Fiery Brand", "player") && FieryBrandCDReady && Fighting && !MetaUp && !DemonSpikesUp){
                Aimsharp.Cast("Fiery Brand");
                return true;
            }

            if (!SaveMeta)
            {
            if (Aimsharp.CanCast("Metamorphosis", "player") && Fighting && MetaCDReady){
            if (!MetaUp && selfhealth <= 30 || Venthyr && !SinfulDebuffUp && EnemiesInMelee >= 3){
                Aimsharp.Cast("Metamorphosis");
                return true;}
                }
            }

            //Covenants
            if (!SaveCovenant){

            if (Venthyr && Aimsharp.CanCast("Sinful Brand") && !SinfulDebuffUp){
            if (EnemiesInMelee <=2 || EnemiesInMelee >=3 && MetaCDRemaining >= 20000){
                Aimsharp.Cast("Sinful Brand");
                return true;}
            }

            if (Kyrian && Aimsharp.CanCast("Elysian Decree", "player") && SigilDrop && Time >= 5000){
            if (EnemiesInMelee >= 2 || EnemiesInMelee <=1 && TargetTimeToDie >= 30 || TargetIsBoss && TargetTimeToDie >= 15){
                Aimsharp.Cast("Elysian self");
                return true;}
            }

            if (NightFae && Aimsharp.CanCast("The Hunt", "player") && Pulling && !Moving){
            if (EnemiesNearTarget >= 3 && TargetTimeToDie >= 15 || TargetTimeToDie >= 30 || TargetIsBoss && TargetTimeToDie >= 15){
                Aimsharp.Cast("The Hunt");
                return true;}
                }
            }

            //Rotation
            if (Autoinfernal){
            if (Aimsharp.CanCast("Infernal Strike", "player") && InfernalCharges > 1 && Fighting && !Moving){
                Aimsharp.Cast("infernal self");
                return true;}
            }

            if (Aimsharp.CanCast("Fel Devastation", "player") && Fighting && FelDevastationCDReady){
            if (EnemiesInMelee >= 3 && Fury >=50 || selfhealth <= 75 && Fury >= 50){
                Aimsharp.Cast("Fel Devastation");
                return true;}
            }

            if (Aimsharp.CanCast("Soul Cleave") && Fighting){
            if (SpiritBombEnabled && !BuffSoulFragmentUp || !SpiritBombEnabled && TalentFracture && Fury >= 55 || !SpiritBombEnabled && !TalentFracture && Fury >=50){
                Aimsharp.Cast("Soul Cleave");
                return true;}
            }

            if (Aimsharp.CanCast("Immolation Aura", "player") && Fighting && ImmolationAuraCDReady){
                Aimsharp.Cast("Immolation Aura");
                return true;
            }

            if (Aimsharp.CanCast("Felblade") && FelbladeCDReady && Fury <= 60 && RangeToTarget <= 15 && TalentFelblade){
                Aimsharp.Cast("Felblade");
                return true;
            }

            if (Aimsharp.CanCast("Fracture", "player") && Fighting && TalentFracture){
                Aimsharp.Cast("Fracture");
                return true;
            }

            if (Aimsharp.CanCast("Sigil of Flame", "player") && SigilDrop && SigilCDReady){
            if (!LegendaryRazelikhsDefilement && Kyrian || LegendaryRazelikhsDefilement && !Kyrian || !Kyrian && !LegendaryRazelikhsDefilement){
                Aimsharp.Cast("sigil self");
                return true;}
            }

            if (Aimsharp.CanCast("Shear") && !TalentFracture && Fighting){
                Aimsharp.Cast("Shear");
                return true;
            }

            if (Aimsharp.CanCast("Throw Glaive") && Pulling){
            if (LegendaryFelBombardment && FelBombardmentStacks == 5 && BuffImmolationAuraUp){
                Aimsharp.Cast("Throw Glaive");
                return true;}
            }

            return false;
            }


        public override bool OutOfCombatTick()
        {
            return false;
        }
    }
}
