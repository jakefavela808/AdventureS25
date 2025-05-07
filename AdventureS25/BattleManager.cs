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
    private static Random rng = new Random();
    private static string? previousLocationAudio = null; 

    public static bool IsBattleActive => isBattleActive;
    public static Pal? PlayerPal => playerPal;
    public static Pal? WildPal => wildPal;

    public static void StartBattle(Pal player, Pal wild)
    {
        isBattleActive = true;
        _lastBattleWonByTaming = false; 
        previousLocationAudio = Player.CurrentLocation?.AudioFile; // Avoid null dereference
        AudioManager.Stop();
        AudioManager.PlayLooping("BattleMusic.wav");

        var availablePals = Player.GetAvailablePals();
        if (availablePals.Count == 0)
        {
            Typewriter.TypeLine("All your Pals are fainted! You cannot battle until you heal them.");
            isBattleActive = false;
            playerPal = null;
            wildPal = null;
            return;
        }
        if (availablePals.Count == 1)
        {
            playerPal = availablePals[0];
        }
        else
        {
            playerPal = Player.PromptPalSelection(availablePals, "Which Pal do you want to send out for battle?");
        }
        wildPal = wild;
        if (wildPal != null) 
        {
            wildPal.HP = wildPal.MaxHP; // Wild Pal HP is reset for each new encounter
            wildPal.BasicAttackUses = wildPal.MaxBasicAttackUses; // Reset wild Pal's basic attack uses
            wildPal.SpecialAttackUses = wildPal.MaxSpecialAttackUses; // Reset wild Pal's special attack uses
        }
        if (playerPal != null)
        {
            Typewriter.TypeLine($"You send out {playerPal.Name}!");
        }
        else
        {
            Typewriter.TypeLine("No Pal was selected to send out!");
            isBattleActive = false;
            wildPal = null;
            return;
        }
    }

    // private static bool playerDefending = false;
    private static bool enemyDefending = false;

    public static void HandlePlayerAction(string action)
    {
        if (!isBattleActive || playerPal == null || wildPal == null) return;
        if (playerPal.HP <= 0)
        {
            var availablePals = Player.GetAvailablePals();
            if (availablePals.Count > 0)
            {
                if (availablePals.Count == 1)
                {
                    playerPal = availablePals[0];
                }
                else
                {
                    playerPal = Player.PromptPalSelection(availablePals, "Your Pal fainted! Choose another Pal to continue the fight:");
                }
                if (playerPal != null)
                {
                    Typewriter.TypeLine($"{playerPal.Name} enters the battle!");
                }
                else
                {
                    Typewriter.TypeLine("No Pal was selected to enter the battle!");
                    EndBattle();
                    States.ChangeState(StateTypes.Exploring);
                    Player.Look();
                    AudioManager.Stop();
                    AudioManager.PlayLooping(previousLocationAudio);
                    return;
                }
            }
            else
            {
                Typewriter.TypeLine("All your Pals are fainted! You cannot continue the battle.");
                EndBattle();
                States.ChangeState(StateTypes.Exploring);
                Player.Look();
                AudioManager.Stop();
                AudioManager.PlayLooping(previousLocationAudio);
                return;
            }
        }
        switch (action)
        {
            case "basic":
                DoAttack(playerPal, wildPal, playerPal.Moves?[0] ?? "Basic Attack");
                break;
            case "special":
                DoAttack(playerPal, wildPal, playerPal.Moves?.Count > 1 ? playerPal.Moves[1] : "Special Attack", isSpecial:true);
                break;
            case "defend":
                // playerDefending = true;
                int heal = 5;
                playerPal.HP = Math.Min(playerPal.MaxHP, playerPal.HP + heal);
                Console.WriteLine("");
                Typewriter.TypeLine($"{playerPal.Name} braces for the next attack and heals for {heal} HP!");
                break;
            case "potion":
                if (Player.HasItem("potion"))
                {
                    HealPal(playerPal);
                    Player.RemoveItemFromInventory("potion");
                    Typewriter.TypeLine("You used a potion!");
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
                    Typewriter.TypeLine("You failed to run away!\n");
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
        int damage = 10 + (isSpecial ? 5 : 0);
        if (halveDamage)
        {
            damage = (int)Math.Ceiling(damage / 2.0);
        }
        defender.HP -= damage;
        if (halveDamage)
        {
            Typewriter.TypeLine($"{attacker.Name} used {move}! {defender.Name} braced and took only {damage} damage.");
        }
        else
        {
            Typewriter.TypeLine($"{attacker.Name} used {move}! {defender.Name} took {damage} damage.");
        }
    }

    private static void HealPal(Pal pal)
    {
        int heal = 15;
        pal.HP = Math.Min(pal.MaxHP, pal.HP + heal);
        Typewriter.TypeLine($"{pal.Name} healed for {heal} HP!");
    }

    private static bool TryTame(Pal wild)
    {
        if (wild == null)
        {
            Typewriter.TypeLine("There is no wild Pal to tame.");
            Console.Clear();
            Player.Look();
            return false;
        }

        // Check for Treat in inventory
        if (!Player.HasItem("Treat"))
        {
            Typewriter.TypeLine("You don't have any treats!");
            return false;
        }

        Player.RemoveItemFromInventory("Treat"); // Consume one Treat
        Typewriter.TypeLine("You use a Treat.....");
        int chance = 30 + (100 * (wild.MaxHP - wild.HP) / wild.MaxHP);
        int roll = rng.Next(0, 100);
        if (roll < chance)
        {
            Typewriter.TypeLine($"You tamed {wild.Name}! It joins your team.");
            Player.AddPal(wild);
            Player.CurrentLocation?.RemovePal(wild);
            _lastBattleWonByTaming = true; 
            EndBattle();
            States.ChangeState(StateTypes.Exploring);
            AudioManager.Stop();
            AudioManager.PlayLooping(previousLocationAudio);

            Player.Look();
            CheckAndTriggerFirstWinTutorial();
            
        }
        else
        {
            Typewriter.TypeLine($"Taming failed!");
        }
        return true;
    }

    private static void EnemyTurn()
    {
        if (wildPal == null || playerPal == null) return;
        int action = rng.Next(0, 3); // 0: basic, 1: special, 2: defend

        if (action == 2) 
        {
            int heal = 5;
            wildPal.HP = Math.Min(wildPal.MaxHP, wildPal.HP + heal);
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
                if (enemyDefending)
                {
                    DoAttack(wildPal, playerPal, move, useSpecial, halveDamage:true);
                    enemyDefending = false;
                }
                else
                {
                    DoAttack(wildPal, playerPal, move, useSpecial);
                }
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
            States.ChangeState(StateTypes.Exploring);
            AudioManager.Stop();
            AudioManager.PlayLooping(previousLocationAudio);
            Player.Look();
            CheckAndTriggerFirstWinTutorial();
        }
        if (playerPal?.HP <= 0)
        {
            var availablePals = Player.GetAvailablePals();
            if (availablePals.Count > 0)
            {
                Typewriter.TypeLine($"{playerPal.Name} fainted! Choose another Pal to continue the fight.");
                playerPal = availablePals[0]; // Auto-select for now; could prompt user for choice
                if (playerPal != null)
{
    Typewriter.TypeLine($"{playerPal.Name} enters the battle!");
}
else
{
    Typewriter.TypeLine("No Pal was selected to enter the battle!");
    EndBattle();
    States.ChangeState(StateTypes.Exploring);
    Player.Look();
    AudioManager.Stop();
    AudioManager.PlayLooping(previousLocationAudio);
    return;
}
            }
            else
            {
                Typewriter.TypeLine("All your Pals are fainted! You lost the battle.");
                EndBattle();
                States.ChangeState(StateTypes.Exploring);
                Player.Look();
                AudioManager.Stop();
                AudioManager.PlayLooping(previousLocationAudio);
            }
        }
    }

    private static void CheckAndTriggerFirstWinTutorial()
    {
        if (Conditions.IsTrue(ConditionTypes.HasReceivedStarter) && Conditions.IsFalse(ConditionTypes.HasDefeatedFirstPal))
        {
            Conditions.ChangeCondition(ConditionTypes.HasDefeatedFirstPal, true);
            Conditions.ChangeCondition(ConditionTypes.PlayerNeedsFirstHealFromNoelia, true); // Set Noelia's trigger
            Typewriter.TypeLine("\nYour Pal looks tired! Go heal it at the Pal Center in town.");
        }
    }

    private static void EndBattle()
    {
        if (playerPal != null && playerPal.HP > 0) // Check if player's Pal is conscious
        {
            if (_lastBattleWonByTaming)
            {
                int xpGained = 75;
                playerPal.AddExperience(xpGained);
                Typewriter.TypeLine($"{playerPal.Name} gained {xpGained} XP for successfully taming {wildPal?.Name}!");
            }
            else if (wildPal != null && wildPal.HP <= 0) // Check if battle won by defeating wild pal
            {
                int xpGained = 50;
                playerPal.AddExperience(xpGained);
                Typewriter.TypeLine($"{playerPal.Name} gained {xpGained} XP for defeating {wildPal.Name}!");
            }
        }

        isBattleActive = false;
        playerPal = null;
        wildPal = null;
        _lastBattleWonByTaming = false; 
    }

    private static void PrintHpStatus()
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
        if (wildPal != null)
        {
            Console.WriteLine("");
            Typewriter.TypeLine($"{wildPal.Name} HP: {wildPal.HP}/{wildPal.MaxHP} | Basic Energy: {wildPal.BasicAttackUses}/{wildPal.MaxBasicAttackUses} | Special Energy: {wildPal.SpecialAttackUses}/{wildPal.MaxSpecialAttackUses}");
        }
        else
        {
            Typewriter.TypeLine("Wild Pal: Fainted or Missing");
        }
        Console.WriteLine(""); // Add this line for an extra blank line after status
    }
}
