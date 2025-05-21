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
    private static bool playerDefending = false; // Declare playerDefending flag
    private static bool enemyDefending = false; 
    private static Random rng = new Random();
    private static string? previousLocationAudio = null; 
    private static bool _isTrainerBattle = false; // New field to track if battle is against a trainer
    private static bool _saulJustDefeated = false; // Flag for Saul's defeat message

    public static bool IsBattleActive => isBattleActive;
    public static Pal? PlayerPal => playerPal;
    public static Pal? WildPal => wildPal;

    public static void StartBattle(Pal? player, Pal opponent, bool isTrainerBattle = false, bool isMidTrainerBattleSwitch = false)
    {
        _isTrainerBattle = isTrainerBattle;
        isBattleActive = true;
        _lastBattleWonByTaming = false;

        // Opponent setup - always happens
        wildPal = opponent;
        if (wildPal != null)
        {
            wildPal.HP = wildPal.MaxHP; // Reset HP for new opponent
            wildPal.ResetAttackUses();  // Reset attack uses
        }

        if (!isMidTrainerBattleSwitch)
        {
            previousLocationAudio = Player.CurrentLocation?.AudioFile;
        }

        // Determine Player's Pal
        if (player != null && player.HP > 0) // Player's Pal was pre-selected and is healthy
        {
            playerPal = player;
        }
        else
        {
            playerPal = Player.GetAvailablePals().FirstOrDefault();
            if (playerPal == null) // No available Pals
            {
                Typewriter.TypeLine("All your Pals have fainted! You cannot battle.");
                isBattleActive = false;
                States.ChangeState(StateTypes.Exploring);
                return;
            }
            // Only announce "You send out..." if it's not a mid-battle switch where the player's Pal hasn't changed.
            // If it IS a mid-battle switch, the player's Pal is assumed to be the same one continuing.
            if (!isMidTrainerBattleSwitch) 
            {
                 Typewriter.TypeLine($"You send out {playerPal.Name}!");
            }
        }

        States.ChangeState(StateTypes.Fighting);

        if (!isMidTrainerBattleSwitch) // Full Battle Start display
        {
            AudioManager.Stop();
            AudioManager.PlayLooping("BattleMusic.wav");
            AudioManager.PlaySoundEffect("BattleBegin.wav");
            Typewriter.TypeLineWithDuration("================ BATTLE BEGIN ================", 1000);

            // Display details for the selected playerPal
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
            Typewriter.TypeLine(""); // Blank line for spacing

            // Display details for the opponent Pal
            Console.WriteLine(CombatCommandHandler.GetAsciiArt(wildPal.AsciiArt));
            Typewriter.TypeLine($"{wildPal.Description}"); // Opponent description
            Typewriter.TypeLine($"HP: {wildPal.HP}/{wildPal.MaxHP}"); // Opponent HP
            if (wildPal.Moves != null && wildPal.Moves.Count > 0)
            {
                string basicMoveDisplay = wildPal.Moves.Count > 0 ? $"{wildPal.Moves[0]} ({wildPal.BasicAttackUses}/{wildPal.MaxBasicAttackUses})" : "N/A";
                string specialMoveDisplay = wildPal.Moves.Count > 1 ? $"{wildPal.Moves[1]} ({wildPal.SpecialAttackUses}/{wildPal.MaxSpecialAttackUses})" : "N/A";
                Typewriter.TypeLine($"Moves: {basicMoveDisplay}, {specialMoveDisplay}");
            }
            Typewriter.TypeLine(""); // Blank line for spacing
        }
        else // Mid-Trainer Battle Switch display
        {
            Console.Clear();
            if (Game.ActiveTrainer != null && wildPal != null)
            {
                Typewriter.TypeLine($"{Game.ActiveTrainer.Name} sent out {wildPal.Name}!");
            }
            else if (wildPal != null) // Fallback if ActiveTrainer somehow null but it's a switch
            {
                Typewriter.TypeLine($"Opponent sent out {wildPal.Name}!");
            }
            Typewriter.TypeLineWithDuration("----------------------------------------------------", 250); // Shorter visual separator
        }

        // Common display for both scenarios
        PrintHpStatus();
        Console.WriteLine(CommandList.combatCommands);
    }

    public static void HandlePlayerAction(string action)
    {
        if (!isBattleActive || playerPal == null || wildPal == null) return;

        // Reset player defending status at the start of their action, unless they choose to defend again
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
                        enemyDefending = false; // Enemy's defense is used up
                    }
                    DoAttack(playerPal, wildPal, playerPal.Moves?[0] ?? "Basic Attack", halveDamage: applyEnemyDefense);
                }
                break;
            case "special":
                {
                    bool applyEnemyDefense = enemyDefending;
                    if (applyEnemyDefense)
                    {
                        enemyDefending = false; // Enemy's defense is used up
                    }
                    DoAttack(playerPal, wildPal, playerPal.Moves?.Count > 1 ? playerPal.Moves[1] : "Special Attack", isSpecial: true, halveDamage: applyEnemyDefense);
                }
                break;
            case "defend":
                playerDefending = true; // Set player defending status
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
                if (_isTrainerBattle) // Check if it's a trainer battle
                {
                    Typewriter.TypeLine("You cannot run from a trainer battle!");
                    // Player does not lose a turn, simply re-prompt for a valid command.
                    // The game loop will handle re-prompting.
                    return; 
                }
                int runChance = 45;
                int roll = rng.Next(0, 100);
                if (roll < runChance)
                {
                    Typewriter.TypeLine("You successfully ran away!");
                    EndBattle();
                    States.ChangeState(StateTypes.Exploring);
                    Player.Look();
                    AudioManager.Stop();
                    AudioManager.PlayLooping(previousLocationAudio);
                    return;
                }
                else
                {
                    Typewriter.TypeLine("You failed to run away!");
                    // Enemy gets a turn if run fails, then we proceed to normal turn flow
                }
                break;
        }

        if (wildPal != null && wildPal.HP > 0 && isBattleActive) // Check isBattleActive again in case run succeeded and changed it
            EnemyTurn();
        CheckBattleEnd();

        // Print final status for the round if battle is still active
        if (isBattleActive && playerPal != null && wildPal != null)
        {
            PrintHpStatus();
        }
    }

    private static void DoAttack(Pal attacker, Pal defender, string move, bool isSpecial = false, bool halveDamage = false)
    {
        Console.WriteLine("");
        int baseDamage = isSpecial ? attacker.BaseSpecialAttackDamage : attacker.BaseAttackDamage;
        int damageVariance = rng.Next(-2, 3); // Random value between -2 and 2 inclusive
        int finalDamage = Math.Max(1, baseDamage + damageVariance); // Ensure damage is at least 1

        if (halveDamage)
        {
            finalDamage = (int)Math.Ceiling(finalDamage / 2.0);
        }
        defender.HP -= finalDamage;
        if (halveDamage)
        {
            AudioManager.PlaySoundEffect("Heal Sound Effect.wav"); // Consider a specific "defend_impact.wav"
            Typewriter.TypeLine($"{attacker.Name} used {move}! {defender.Name} braced and took only {finalDamage} damage.");
        }
        else
        {
            AudioManager.PlaySoundEffect("BasicAttack.wav"); // Consider different sounds for special attacks
            Typewriter.TypeLine($"{attacker.Name} used {move}! {defender.Name} took {finalDamage} damage.");
        }
    }

    private static void HealPal(Pal pal)
    {
        int baseHeal = 15;
        int healVariance = rng.Next(-2, 3); // Random value between -2 and 2 inclusive
        int finalHeal = Math.Max(5, baseHeal + healVariance); // Ensure heal is at least 5 (or some other minimum)

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

        // Check for Treat in inventory
        if (!Player.HasItem("Treat"))
        {
            Typewriter.TypeLine("You don't have any treats!");
            return false;
        }

        Player.RemoveItemFromInventory("Treat"); // Consume one Treat
        Typewriter.TypeLine("You use a Treat...");
        int chance = 30 + (100 * (wild.MaxHP - wild.HP) / wild.MaxHP); // Taming chance increases as Pal HP decreases
        int roll = rng.Next(0, 100);
        if (roll < chance)
        {
            AudioManager.PlaySoundEffect("Tamed.wav");
            Typewriter.TypeLine($"You tamed {wild.Name}! It joins your team.");
            Player.AddPal(wild);
            _lastBattleWonByTaming = true; 
            EndBattle();
            return true; // Return true after successful taming and battle end
        }
        else
        {
            Typewriter.TypeLine($"Taming failed!");
            return false; // Return false if taming fails, allowing enemy turn
        }
    }

    private static void EnemyTurn()
    {
        if (wildPal == null || playerPal == null) return;
        int action = rng.Next(0, 3); // 0: basic, 1: special, 2: defend

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
            // If chosen attack is out of energy, try the other one. If both are out, defend.
            else if (useSpecial && wildPal.BasicAttackUses > 0) // Tried special, was out, try basic
            {
                move = wildPal.Moves?[0] ?? "Wild Basic Attack";
                wildPal.BasicAttackUses--;
                useSpecial = false; // Switched to basic
                canAttack = true;
            }
            else if (!useSpecial && wildPal.SpecialAttackUses > 0) // Tried basic, was out, try special
            {
                move = wildPal.Moves?.Count > 1 ? wildPal.Moves[1] : "Wild Special Attack";
                wildPal.SpecialAttackUses--;
                useSpecial = true; // Switched to special
                canAttack = true;
            }
            else // Out of energy for both, or no moves defined
            {
                int heal = 5;
                wildPal.HP = Math.Min(wildPal.MaxHP, wildPal.HP + heal);
                Typewriter.TypeLine($"{wildPal.Name} seems out of energy and braces for the next attack, healing for {heal} HP!");
                enemyDefending = true;
                return; // Skip attack logic
            }

            if (canAttack)
            {
                // Determine if player is defending to halve damage
                bool applyPlayerDefense = playerDefending;
                if (applyPlayerDefense)
                {
                    playerDefending = false; // Player's defense is used up for this attack
                }
                DoAttack(wildPal, playerPal, move, useSpecial, halveDamage: applyPlayerDefense);
            }
        }
    }

    private static void CheckBattleEnd()
    {
        if (!isBattleActive) return;

        // Check for wild Pal fainted (Opponent fainted)
        if (wildPal?.HP <= 0)
        {
            Typewriter.TypeLine($"{wildPal.Name} fainted!");
            EndBattle(); // EndBattle will now handle all further logic, including state changes
            return; 
        }

        // Check for player Pal fainted
        if (playerPal?.HP <= 0)
        {
            if (AttemptPlayerPalSwitch()) // True if switch was successful
            {
                // Pal switched successfully, battle continues.
                // Player gets to act with new Pal.
                return; // Do not end battle, player gets to act with new Pal.
            }
            else // AttemptPlayerPalSwitch returned false (no Pals left)
            {
                // No Pals left, player loses. EndBattle will handle this.
                EndBattle();
                return; 
            }
        }
    }

    private static void CheckAndTriggerFirstWinTutorial()
    {
        if (Conditions.IsTrue(ConditionTypes.HasReceivedStarter) && Conditions.IsFalse(ConditionTypes.HasDefeatedFirstPal))
        {
            Conditions.ChangeCondition(ConditionTypes.HasDefeatedFirstPal, true);
            Conditions.ChangeCondition(ConditionTypes.PlayerNeedsFirstHealFromNoelia, true); // Set Noelia's trigger
            AudioManager.PlaySoundEffect("GoHeal.wav");
            Typewriter.TypeLineWithDuration("\nGreat job on your first battle! Your Pal looks tired, go heal it at the Pal Center.", 7000);
        }
    }

    private static void EndBattle()
    {
        // This method is now the central point for ending a round or the entire battle.

        // Case 1: Trainer Battle In Progress and needs to continue or conclude
        if (_isTrainerBattle && Game.ActiveTrainer != null && Game.ActiveTrainerParty != null)
        {
            bool trainerPalFainted = wildPal?.HP <= 0; // wildPal is the trainer's current Pal
            // Check if all player Pals are fainted, considering the current playerPal might be the one that just fainted.
            bool playerAllPalsEffectivelyFainted = Player.GetAvailablePals().Count == 0 && (playerPal == null || playerPal.HP <= 0);

            if (trainerPalFainted)
            {
                // Award XP for defeating this specific trainer Pal
                if (playerPal != null && playerPal.HP > 0) // Player's Pal must be conscious
                {
                    int xpGained = 60; // XP for defeating one trainer Pal
                    playerPal.AddExperience(xpGained);
                }

                Game.CurrentTrainerPalIndex++;
                if (Game.CurrentTrainerPalIndex < Game.ActiveTrainerParty.Count)
                {
                    // Trainer has more Pals, continue battle
                    Pal nextTrainerPal = Game.ActiveTrainerParty[Game.CurrentTrainerPalIndex];
                    
                    // Ensure the player's current Pal is fit or handle switch if needed
                    Pal? currentFightingPlayerPal = playerPal;
                    if (currentFightingPlayerPal == null || currentFightingPlayerPal.HP <= 0)
                    {
                        // This case implies the player's Pal fainted simultaneously or just before.
                        // AttemptPlayerPalSwitch should have been called by CheckBattleEnd.
                        // If we are here, it means a switch might have occurred or is needed.
                        currentFightingPlayerPal = Player.GetAvailablePals().FirstOrDefault();
                        if (currentFightingPlayerPal == null) {
                            // All player pals fainted, this will be caught by playerAllPalsEffectivelyFainted next.
                            // For safety, trigger that logic path if somehow missed.
                            playerAllPalsEffectivelyFainted = true; 
                        }
                    }
                    
                    if (!playerAllPalsEffectivelyFainted) // Check again if player still has Pals
                    {
                        Typewriter.TypeLine($"Trainer {Game.ActiveTrainer.Name} is about to send out {nextTrainerPal.Name}!");
                        System.Threading.Thread.Sleep(1500); // Short pause
                        
                        StartBattle(currentFightingPlayerPal, nextTrainerPal, isTrainerBattle: true, isMidTrainerBattleSwitch: true);
                        return; // Battle continues with next trainer Pal, DO NOT proceed to cleanup
                    }
                    // If player has no pals, fall through to playerAllPalsEffectivelyFainted logic below
                }
                else
                {
                    // All trainer's Pals defeated - Player wins trainer battle!
                    if(Game.ActiveTrainer.Name == "Trainer Saul")
                    {
                        Typewriter.TypeLine("Saul: There's no way you defeated me!!");
                        System.Threading.Thread.Sleep(1500); // Pause for effect
                        _saulJustDefeated = true; // Set flag for end-of-game message
                        Conditions.ChangeCondition(ConditionTypes.DefeatedTrainerSaul, true);
                    }
                    Typewriter.TypeLine($"You defeated Trainer {Game.ActiveTrainer.Name}!");
                    // Use the specific condition for Saul, or a general one later
                    // if(Game.ActiveTrainer.Name == "Trainer Saul") Conditions.ChangeCondition(ConditionTypes.DefeatedTrainerSaul, true); // Moved up for Saul
                    
                    int overallXpReward = 150; 
                    Typewriter.TypeLine($"You gained an additional {overallXpReward} XP for the victory!");
                    var consciousPlayerPals = Player.Pals.Where(p => p.HP > 0).ToList();
                    if (consciousPlayerPals.Any())
                    {
                        int xpPerPal = overallXpReward / consciousPlayerPals.Count;
                        foreach(var p in consciousPlayerPals) { p.AddExperience(xpPerPal); }
                    }
                    // TODO: Add other rewards like money, items.
                    // Dialogue like "Saul: Hmph! Lucky shot..." is now in ConversationCommandHandler post-battle check.
                }
            }
            
            // This check needs to be robust. If trainerPalFainted led to victory, this shouldn't execute for loss.
            // So, ensure playerAllPalsEffectivelyFainted is checked if the trainer battle didn't just end in player victory.
            if (playerAllPalsEffectivelyFainted && Game.ActiveTrainerParty != null) // Check ActiveTrainerParty to ensure we are still in trainer context that hasn't been cleared by victory
            {
                // Player lost the trainer battle
                Typewriter.TypeLine($"All your Pals have fainted! You were defeated by Trainer {Game.ActiveTrainer.Name}!");
                // Dialogue like "Saul: Hah! You're weak!" is now in ConversationCommandHandler post-battle check.
            }
            
            // If trainer battle is resolved (win or loss), clear trainer-specific state
            if ((trainerPalFainted && Game.CurrentTrainerPalIndex >= Game.ActiveTrainerParty.Count) || playerAllPalsEffectivelyFainted)
            {
                Game.ActiveTrainer = null;
                Game.ActiveTrainerParty = null;
                // _isTrainerBattle will be reset in general cleanup if battle is fully over
            }
            else if (!trainerPalFainted && !playerAllPalsEffectivelyFainted)
            {
                // Battle is not over (e.g. player switched Pal, enemy Pal still up). This should have been handled by CheckBattleEnd returning.
                // If EndBattle is called in this state, it's likely an issue. For safety, return if battle should continue.
                // This path indicates the current Pal vs Pal round is over, but not due to fainting of either, which is odd for trainer battles if run is disabled.
                // This block is a safeguard; ideally, CheckBattleEnd handles intermediate states.
                return; 
            }
        }
        // Case 2: Wild (non-trainer) Pal was defeated
        else if (!_isTrainerBattle && wildPal?.HP <= 0 && !_lastBattleWonByTaming) 
        { // Ensure not trainer, wild pal fainted, and not by taming
            if (playerPal != null && playerPal.HP > 0)
            {
                int xpGained = 50;
                playerPal.AddExperience(xpGained);
            }
            Console.Clear();
            Player.Look();
        }
        // Case 3: Wild Pal Tamed (and not a trainer battle)
        else if (!_isTrainerBattle && _lastBattleWonByTaming && wildPal != null) 
        {
             if (playerPal != null && playerPal.HP > 0)
             {
                int xpGained = 75;
                playerPal.AddExperience(xpGained);
             }
            // Track tamed Pal in its location
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
        // Case 4: Player lost to a wild Pal (all player Pals fainted)
        else if (!_isTrainerBattle && Player.GetAvailablePals().Count == 0 && (playerPal == null || playerPal.HP <= 0))
        {
            // Message "All your Pals have fainted!" is handled by AttemptPlayerPalSwitch
            // No specific XP or reward here, just cleanup.
        }

        // General Battle Cleanup - This happens if the battle is TRULY over and not continuing (e.g. next trainer Pal).
        isBattleActive = false;
        States.ChangeState(StateTypes.Exploring);
        AudioManager.Stop(); 
        if (!string.IsNullOrEmpty(previousLocationAudio))
        {
            AudioManager.PlayLooping(previousLocationAudio);
        }
        else if(Player.CurrentLocation?.AudioFile != null)
        {
            AudioManager.PlayLooping(Player.CurrentLocation.AudioFile);
        }
        
        // Clear active wild Pal from location if it was a non-trainer, non-tamed wild encounter that concluded.
        if (!_isTrainerBattle && Player.CurrentLocation != null && !_lastBattleWonByTaming && (wildPal == null || wildPal.HP <=0) )
        {
            Player.CurrentLocation.ActiveWildPal = null;
        }

        // Reset all battle-specific state variables
        playerPal = null; 
        wildPal = null; 
        _lastBattleWonByTaming = false;
        _isTrainerBattle = false; // Reset for the next potential battle
        playerDefending = false;
        enemyDefending = false;
        // Ensure trainer state is cleared if somehow missed by specific trainer logic paths
        if (Game.ActiveTrainer != null) Game.ActiveTrainer = null; 
        if (Game.ActiveTrainerParty != null) Game.ActiveTrainerParty = null;

        Player.Look(); 
        Console.Clear(); // Clear console before tutorial message for better visibility
        CheckAndTriggerFirstWinTutorial(); // Moved to the very end

        if (_saulJustDefeated)
        {
            _saulJustDefeated = false; // Consume the flag
            Console.Clear();
            Typewriter.TypeLine("==============================================");
            Typewriter.TypeLine("Congrats, you win!");
            Typewriter.TypeLine("Thank you for playing AdventureS25!");
            Typewriter.TypeLine("==============================================");
            // Potentially add a game over state or prompt to exit here in a fuller game.
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

    // New method to handle player Pal switching
    private static bool AttemptPlayerPalSwitch()
    {
        if (playerPal == null) return false; // Should not happen if called after a Pal faints

        Typewriter.TypeLine($"{playerPal.Name} fainted!");
        var availablePals = Player.GetAvailablePals();

        if (availablePals.Count == 0)
        {
            Typewriter.TypeLine("All your Pals have fainted!");
            return false; // No Pals left to switch to
        }

        if (availablePals.Count == 1)
        {
            playerPal = availablePals[0];
        }
        else
        {
            playerPal = Player.PromptPalSelection(availablePals, "Choose another Pal to continue the fight:");
        }

        if (playerPal != null)
        {
            Typewriter.TypeLine($"{playerPal.Name} enters the battle!");
            // Reset defending status for the new Pal
            playerDefending = false; 
            return true; // Switch successful
        }
        else
        {
            // This case should ideally not be reached if PromptPalSelection always forces a choice
            // or if GetAvailablePals ensures list isn't empty before prompting.
            Typewriter.TypeLine("No Pal was selected to enter the battle! This shouldn't happen.");
            return false; // Switch failed
        }
    }
}
