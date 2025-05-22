using System;
using System.Collections.Generic;
using System.Threading; 
using AdventureS25;

public static class BattleManager
{
    private static Pal? playerPal;
    private static Pal? wildPal;
    private static bool isBattleActive = false;
    private static bool _lastBattleWonByTaming = false; 
    private static bool playerDefending = false;
    private static bool enemyDefending = false; 
    private static Random rng = new Random();
    private static string? previousLocationAudio = null; 
    private static bool _isTrainerBattle = false;
    private static bool _saulJustDefeated = false;

    public static bool IsBattleActive => isBattleActive;
    public static Pal? PlayerPal => playerPal;
    public static Pal? WildPal => wildPal;

    public static void StartBattle(Pal? player, Pal opponent, bool isTrainerBattle = false, bool isMidTrainerBattleSwitch = false)
    {
        _isTrainerBattle = isTrainerBattle;
        isBattleActive = true;
        _lastBattleWonByTaming = false;
        wildPal = opponent;
        if (wildPal != null)
        {
            wildPal.HP = wildPal.MaxHP;
            wildPal.ResetAttackUses();
            if (isTrainerBattle)
            {
                wildPal.IsAcquirable = false;
            }
        }

        if (!isMidTrainerBattleSwitch)
        {
            previousLocationAudio = Player.CurrentLocation?.AudioFile;
            var availablePals = Player.GetAvailablePals();

            if (availablePals.Count == 0)
            {
                Typewriter.TypeLine("All your Pals have fainted! You cannot battle.");
                isBattleActive = false;
                States.ChangeState(StateTypes.Exploring);
                if (previousLocationAudio != null) AudioManager.PlayLooping(previousLocationAudio);
                return;
            }
            else if (availablePals.Count == 1)
            {
                playerPal = availablePals.First();
                Typewriter.TypeLine($"You send out {playerPal.Name}!");
            }
            else
            {
                Typewriter.TypeLine("\nWhich Pal do you want to send out?");
                for (int i = 0; i < availablePals.Count; i++)
                {
                    Typewriter.TypeLine($"{i + 1}. {availablePals[i].Name} (Lvl {availablePals[i].Level}, HP: {availablePals[i].HP}/{availablePals[i].MaxHP})");
                }

                int choice = -1;
                while (choice < 1 || choice > availablePals.Count)
                {
                    Console.Write("Enter the number of your choice: ");
                    string? input = Console.ReadLine();
                    if (int.TryParse(input, out int numChoice) && numChoice >= 1 && numChoice <= availablePals.Count)
                    {
                        choice = numChoice;
                    }
                    else
                    {
                        Typewriter.TypeLine("Invalid choice. Please enter a number from the list.");
                    }
                }
                playerPal = availablePals[choice - 1];
                Typewriter.TypeLine($"You send out {playerPal.Name}!");
            }
        }
        else
        {
            if (playerPal == null || playerPal.HP <= 0)
            {
                Typewriter.TypeLine("Error: Player's active Pal is unexpectedly unavailable during opponent switch.");
                playerPal = Player.GetAvailablePals().FirstOrDefault();
                if (playerPal == null)
                {
                    Typewriter.TypeLine("All your Pals have fainted! Battle ends.");
                    EndBattle();
                    return;
                }
                Typewriter.TypeLine($"Auto-selecting {playerPal.Name} to continue.");
            }
        }
        if (playerPal == null)
        {
            Typewriter.TypeLine("Critical Error: No player Pal selected for battle.");
            isBattleActive = false;
            States.ChangeState(StateTypes.Exploring);
            if (previousLocationAudio != null && !isMidTrainerBattleSwitch) AudioManager.PlayLooping(previousLocationAudio);
            return;
        }

        States.ChangeState(StateTypes.Fighting);

        if (!isMidTrainerBattleSwitch)
        {
            AudioManager.Stop();
            AudioManager.PlayLooping("BattleMusic.wav");
            AudioManager.PlaySoundEffect("BattleBegin.wav");
            Typewriter.TypeLineWithDuration("================ BATTLE BEGIN ================", 1000);
            Console.WriteLine(CombatCommandHandler.GetAsciiArt(playerPal.AsciiArt));
            Typewriter.TypeLine($"{playerPal.Name} - LVL {playerPal.Level}");
            Typewriter.TypeLine($"{playerPal.Description}");
            Typewriter.TypeLine($"HP: {playerPal.HP}/{playerPal.MaxHP}");
            if (playerPal.Moves != null && playerPal.Moves.Count > 0)
            {
                string basicMoveDisplay = playerPal.Moves.Count > 0 ? $"{playerPal.Moves[0]} ({playerPal.BasicAttackUses}/{playerPal.MaxBasicAttackUses})" : "N/A";
                string specialMoveDisplay = playerPal.Moves.Count > 1 ? $"{playerPal.Moves[1]} ({playerPal.SpecialAttackUses}/{playerPal.MaxSpecialAttackUses})" : "N/A";
                Typewriter.TypeLine($"Moves: {basicMoveDisplay}, {specialMoveDisplay}");
            }
            Typewriter.TypeLine("");

            Console.WriteLine(CombatCommandHandler.GetAsciiArt(wildPal.AsciiArt));
            Typewriter.TypeLine($"{wildPal.Name} - LVL {wildPal.Level}");
            Typewriter.TypeLine($"{wildPal.Description}");
            Typewriter.TypeLine($"HP: {wildPal.HP}/{wildPal.MaxHP}");
            if (wildPal.Moves != null && wildPal.Moves.Count > 0)
            {
                string basicMoveDisplay = wildPal.Moves.Count > 0 ? $"{wildPal.Moves[0]} ({wildPal.BasicAttackUses}/{wildPal.MaxBasicAttackUses})" : "N/A";
                string specialMoveDisplay = wildPal.Moves.Count > 1 ? $"{wildPal.Moves[1]} ({wildPal.SpecialAttackUses}/{wildPal.MaxSpecialAttackUses})" : "N/A";
                Typewriter.TypeLine($"Moves: {basicMoveDisplay}, {specialMoveDisplay}");
            }
            Typewriter.TypeLine("");
        }
        else
        {
            if (Game.ActiveTrainer != null && wildPal != null)
            {
                Typewriter.TypeLine($"{Game.ActiveTrainer.Name} sent out {wildPal.Name}!");
            }
            else if (wildPal != null)
            {
                Typewriter.TypeLine($"Opponent sent out {wildPal.Name}!");
            }
        }

        if (!isMidTrainerBattleSwitch)
        {
            Console.WriteLine(CommandList.combatCommands);
        }

        if (!isMidTrainerBattleSwitch) 
        {
        }
    }

    public static void HandlePlayerAction(string action)
    {
        if (!isBattleActive || playerPal == null || wildPal == null) return;

        bool actionIsDefend = action.ToLower() == "defend";
        if (!actionIsDefend)
        {
            playerDefending = false;
        }

        switch (action)
        {
            case "basic":
                {
                    bool applyEnemyDefense = enemyDefending;
                    if (applyEnemyDefense)
                    {
                        enemyDefending = false;
                    }
                    DoAttack(playerPal, wildPal, playerPal.Moves?[0] ?? "Basic Attack", halveDamage: applyEnemyDefense);
                }
                break;
            case "special":
                {
                    bool applyEnemyDefense = enemyDefending;
                    if (applyEnemyDefense)
                    {
                        enemyDefending = false;
                    }
                    DoAttack(playerPal, wildPal, playerPal.Moves?.Count > 1 ? playerPal.Moves[1] : "Special Attack", isSpecial: true, halveDamage: applyEnemyDefense);
                }
                break;
            case "defend":
                playerDefending = true;
                Console.WriteLine("");
                int heal = 5;
                playerPal.HP = Math.Min(playerPal.MaxHP, playerPal.HP + heal);
                AudioManager.PlaySoundEffect("Heal Sound Effect.wav");
                Typewriter.TypeLine($"{playerPal.Name} braces for the next attack and heals for {heal} HP!");
                break;
            case "potion":
                if (Player.HasItem("potion"))
                {
                    HealPal(playerPal);
                    Player.RemoveItemFromInventory("potion");
                }
                else
                {
                    Typewriter.TypeLine("You don't have any potions!");
                    return; 
                }
                break;
            case "tame":
                if (!TryTame(wildPal))
                    return;
                break;
            case "run":
                if (_isTrainerBattle)
                {
                    Typewriter.TypeLine("You cannot run from a trainer battle!");
                    return; 
                }
                int runChance = 45;
                int roll = rng.Next(0, 100);
                if (roll < runChance)
                {
                    Typewriter.TypeLine("You successfully ran away!");
                    EndBattle();
                    States.ChangeState(StateTypes.Exploring);
                    AudioManager.Stop();
                    AudioManager.PlayLooping(previousLocationAudio);
                    return;
                }
                else
                {
                    Typewriter.TypeLine("You failed to run away!");
                }
                break;
        }

        if (wildPal != null && wildPal.HP > 0 && isBattleActive)
            EnemyTurn();
        CheckBattleEnd();

        if (isBattleActive && playerPal != null && wildPal != null)
        {
            PrintHpStatus();
        }
    }

    private static void DoAttack(Pal attacker, Pal defender, string move, bool isSpecial = false, bool halveDamage = false)
    {
        Console.WriteLine("");
        int baseDamage = isSpecial ? attacker.BaseSpecialAttackDamage : attacker.BaseAttackDamage;
        int damageVariance = rng.Next(-2, 3);
        int finalDamage = Math.Max(1, baseDamage + damageVariance);

        if (halveDamage)
        {
            finalDamage = (int)Math.Ceiling(finalDamage / 2.0);
        }
        defender.HP -= finalDamage;
        if (halveDamage)
        {
            AudioManager.PlaySoundEffect("Heal Sound Effect.wav");
            Typewriter.TypeLine($"{attacker.Name} used {move}! {defender.Name} braced and took only {finalDamage} damage.");
        }
        else
        {
            AudioManager.PlaySoundEffect("BasicAttack.wav");
            Typewriter.TypeLine($"{attacker.Name} used {move}! {defender.Name} took {finalDamage} damage.");
        }
    }

    private static void HealPal(Pal pal)
    {
        int baseHeal = 15;
        int healVariance = rng.Next(-2, 3);
        int finalHeal = Math.Max(5, baseHeal + healVariance);

        pal.HP = Math.Min(pal.MaxHP, pal.HP + finalHeal);
        AudioManager.PlaySoundEffect("Heal Sound Effect.wav");
        Typewriter.TypeLine($"\n{pal.Name} healed for {finalHeal} HP!");
    }

    private static bool TryTame(Pal wild)
    {
        if (wild == null)
        {
            Typewriter.TypeLine("There is no wild Pal to tame.");
            return false;
        }

        if (!Player.HasItem("Treat"))
        {
            Typewriter.TypeLine("You don't have any treats!");
            return false;
        }

        Player.RemoveItemFromInventory("Treat");
        Typewriter.TypeLine("You use a Treat...");
        int chance = 30 + (100 * (wild.MaxHP - wild.HP) / wild.MaxHP);
        int roll = rng.Next(0, 100);
        if (roll < chance)
        {
            AudioManager.PlaySoundEffect("Tamed.wav");
            Typewriter.TypeLine($"You tamed {wild.Name}! It joins your team.");
            Player.AddPal(wild);
            _lastBattleWonByTaming = true; 
            EndBattle();
            return true;
        }
        else
        {
            Typewriter.TypeLine($"Taming failed!");
            return false;
        }
    }

    private static void EnemyTurn()
    {
        if (wildPal == null || playerPal == null) return;
        int action = rng.Next(0, 3);

        if (action == 2)
        {
            Console.WriteLine("");
            int heal = 5;
            wildPal.HP = Math.Min(wildPal.MaxHP, wildPal.HP + heal);
            AudioManager.PlaySoundEffect("Heal Sound Effect.wav");
            Typewriter.TypeLine($"{wildPal.Name} braces for the next attack and heals for {heal} HP!");
            enemyDefending = true;
        }
        else
        {
            bool useSpecial = (action == 1);
            string move;
            bool canAttack = false;

            if (useSpecial && wildPal.SpecialAttackUses > 0)
            {
                move = wildPal.Moves?.Count > 1 ? wildPal.Moves[1] : "Wild Special Attack";
                wildPal.SpecialAttackUses--;
                canAttack = true;
            }
            else if (!useSpecial && wildPal.BasicAttackUses > 0)
            {
                move = wildPal.Moves?[0] ?? "Wild Basic Attack";
                wildPal.BasicAttackUses--;
                canAttack = true;
            }
            else if (useSpecial && wildPal.BasicAttackUses > 0)
            {
                move = wildPal.Moves?[0] ?? "Wild Basic Attack";
                wildPal.BasicAttackUses--;
                useSpecial = false;
                canAttack = true;
            }
            else if (!useSpecial && wildPal.SpecialAttackUses > 0)
            {
                move = wildPal.Moves?.Count > 1 ? wildPal.Moves[1] : "Wild Special Attack";
                wildPal.SpecialAttackUses--;
                useSpecial = true;
                canAttack = true;
            }
            else
            {
                int heal = 5;
                wildPal.HP = Math.Min(wildPal.MaxHP, wildPal.HP + heal);
                Typewriter.TypeLine($"{wildPal.Name} seems out of energy and braces for the next attack, healing for {heal} HP!");
                enemyDefending = true;
                return;
            }

            if (canAttack)
            {
                bool applyPlayerDefense = playerDefending;
                if (applyPlayerDefense)
                {
                    playerDefending = false;
                }
                DoAttack(wildPal, playerPal, move, useSpecial, halveDamage: applyPlayerDefense);
            }
        }
    }

    private static void CheckBattleEnd()
    {
        if (!isBattleActive) return;

        if (wildPal?.HP <= 0)
        {
            Typewriter.TypeLine($"{wildPal.Name} fainted!");
            EndBattle();
            return; 
        }

        if (playerPal?.HP <= 0)
        {
            if (AttemptPlayerPalSwitch())
            {
                return;
            }
            else
            {
                EndBattle();
                return; 
            }
        }
    }

    private static void CheckAndTriggerFirstWinTutorial()
    {
        if (!Conditions.IsTrue(ConditionTypes.HasDefeatedFirstPal) && (wildPal == null || wildPal.HP <= 0))
        {
            Conditions.ChangeCondition(ConditionTypes.HasDefeatedFirstPal, true);
            Conditions.ChangeCondition(ConditionTypes.PlayerNeedsFirstHealFromNoelia, true);
            AudioManager.PlaySoundEffect("GoHeal.wav");
            Typewriter.TypeLineWithDuration("Great job on your first battle! Your Pal looks tired, go heal it at the Pal Center.", 7000);
        }
    }

    private static void EndBattle()
    {
        if (_isTrainerBattle && Game.ActiveTrainer != null && Game.ActiveTrainerParty != null)
        {
            bool trainerPalFainted = wildPal?.HP <= 0;
            bool playerAllPalsEffectivelyFainted = Player.GetAvailablePals().Count == 0 && (playerPal == null || playerPal.HP <= 0);

            if (trainerPalFainted)
            {
                if (playerPal != null && playerPal.HP > 0)
                {
                    int xpGained = 60;
                    playerPal.AddExperience(xpGained);
                }

                Game.CurrentTrainerPalIndex++;
                if (Game.CurrentTrainerPalIndex < Game.ActiveTrainerParty.Count)
                {
                    Pal nextTrainerPal = Game.ActiveTrainerParty[Game.CurrentTrainerPalIndex];
                    
                    Pal? currentFightingPlayerPal = playerPal;
                    if (currentFightingPlayerPal == null || currentFightingPlayerPal.HP <= 0)
                    {
                        currentFightingPlayerPal = Player.GetAvailablePals().FirstOrDefault();
                        if (currentFightingPlayerPal == null) {
                            playerAllPalsEffectivelyFainted = true;
                            playerAllPalsEffectivelyFainted = true; 
                        }
                    }
                    
                    if (!playerAllPalsEffectivelyFainted)
                    {
                        StartBattle(currentFightingPlayerPal, nextTrainerPal, isTrainerBattle: true, isMidTrainerBattleSwitch: true);
                        return;
                    }
                }
                else
                {
                    if(Game.ActiveTrainer.Name == "Trainer Saul")
                    {
                        AudioManager.PlaySoundEffect("SaulDefeat.wav");
                        Typewriter.TypeLineWithDuration("Saul: There's no way you defeated me!!", 3000);
                        _saulJustDefeated = true;
                        Conditions.ChangeCondition(ConditionTypes.DefeatedTrainerSaul, true);
                    }
                }
            }
            if (playerAllPalsEffectivelyFainted && Game.ActiveTrainerParty != null)
            {
                AudioManager.PlaySoundEffect("SaulWin.wav");
                Typewriter.TypeLineWithDuration("Saul: Hah! You're weak! You can't beat me! Come back when you're ready.", 5000);
            }
            
            if ((trainerPalFainted && Game.CurrentTrainerPalIndex >= Game.ActiveTrainerParty.Count) || playerAllPalsEffectivelyFainted)
            {
                Game.ActiveTrainer = null;
                Game.ActiveTrainerParty = null;
            }
            else if (!trainerPalFainted && !playerAllPalsEffectivelyFainted)
            {
                return; 
            }
        }
        else if (!_isTrainerBattle && wildPal?.HP <= 0 && !_lastBattleWonByTaming) 
        {
            if (playerPal != null && playerPal.HP > 0)
            {
                int xpGained = 50;
                playerPal.AddExperience(xpGained);
            }
            Console.Clear();
        }
        else if (!_isTrainerBattle && _lastBattleWonByTaming && wildPal != null) 
        {
             if (playerPal != null && playerPal.HP > 0)
             {
                int xpGained = 75;
                playerPal.AddExperience(xpGained);
             }
            if (Player.CurrentLocation != null) 
            {
                string locationName = Player.CurrentLocation.Name;
                if (!Game.TamedPalsByLocation.ContainsKey(locationName))
                {
                    Game.TamedPalsByLocation[locationName] = new List<string>();
                }
                if (!Game.TamedPalsByLocation[locationName].Contains(wildPal.Name))
                {
                    Game.TamedPalsByLocation[locationName].Add(wildPal.Name);
                }
            }
        }
        else if (!_isTrainerBattle && Player.GetAvailablePals().Count == 0 && (playerPal == null || playerPal.HP <= 0))
        {
            // Message "All your Pals have fainted!" is handled by AttemptPlayerPalSwitch
            // No specific XP or reward here, just cleanup.
        }
        AudioManager.Stop();
        if (!string.IsNullOrEmpty(previousLocationAudio))
        {
            AudioManager.PlayLooping(previousLocationAudio);
        }
        else if (Player.CurrentLocation?.AudioFile != null)
        {
            AudioManager.PlayLooping(Player.CurrentLocation.AudioFile);
        }

        if (!_isTrainerBattle && Player.CurrentLocation != null)
        {
            Player.CurrentLocation.ActiveWildPal = null;
        }

        playerPal = null; 
        wildPal = null;
        _lastBattleWonByTaming = false;
        _isTrainerBattle = false;
        playerDefending = false;
        enemyDefending = false;

        if (Game.ActiveTrainer != null) Game.ActiveTrainer = null; 
        if (Game.ActiveTrainerParty != null) Game.ActiveTrainerParty = null;

        isBattleActive = false;

        if (_saulJustDefeated)
        {
            _saulJustDefeated = false;
            AudioManager.Stop();
            Console.Clear();
            Typewriter.TypeLine("==============================================");
            Typewriter.TypeLine("Congrats, you win!");
            Typewriter.TypeLine("==============================================");
            Game.ContinueMainLoop = false;
        }
        else 
        {
            States.ChangeState(StateTypes.Exploring);
            Console.Clear();
            Player.Look();
            CheckAndTriggerFirstWinTutorial();
        }
    }

    public static void PrintHpStatus()
    {
        Console.WriteLine("");
        if (playerPal != null)
        {
            Typewriter.TypeLine($"{playerPal.Name} HP: {playerPal.HP}/{playerPal.MaxHP} | Basic Energy: {playerPal.BasicAttackUses}/{playerPal.MaxBasicAttackUses} | Special Energy: {playerPal.SpecialAttackUses}/{playerPal.MaxSpecialAttackUses}");
        }
        else
        {
            Typewriter.TypeLine("Player Pal: Fainted or Missing");
        }
        Console.WriteLine("");
        if (wildPal != null)
        {
            Typewriter.TypeLine($"{wildPal.Name} HP: {wildPal.HP}/{wildPal.MaxHP} | Basic Energy: {wildPal.BasicAttackUses}/{wildPal.MaxBasicAttackUses} | Special Energy: {wildPal.SpecialAttackUses}/{wildPal.MaxSpecialAttackUses}");
        }
        else
        {
            Typewriter.TypeLine("Wild Pal: Fainted or Missing");
        }
        Console.WriteLine("");
    }

    private static bool AttemptPlayerPalSwitch()
    {
        if (playerPal == null) return false;

        Typewriter.TypeLine($"{playerPal.Name} fainted!");
        var availablePals = Player.GetAvailablePals();

        if (availablePals.Count == 0)
        {
            Typewriter.TypeLine("All your Pals have fainted!");
            return false;       
        }

        if (availablePals.Count == 1)
        {
            playerPal = availablePals[0];
        }
        else
        {
            playerPal = Player.PromptPalSelection(availablePals, "\nChoose another Pal to continue the fight:");
        }

        if (playerPal != null)
        {
            Typewriter.TypeLine($"{playerPal.Name} enters the battle!");
            playerDefending = false; 
            return true; 
        }
        else
        {
            Typewriter.TypeLine("No Pal was selected to enter the battle! This shouldn't happen.");
            return false; 
        }
    }
}
