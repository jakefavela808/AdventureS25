using System;
using AdventureS25;

public static class BattleManager
{
    private static Pal? playerPal;
    private static Pal? wildPal;
    private static bool isBattleActive = false;
    private static Random rng = new Random();

    public static bool IsBattleActive => isBattleActive;
    public static Pal PlayerPal => playerPal;
    public static Pal WildPal => wildPal;

    public static void StartBattle(Pal player, Pal wild)
    {
        playerPal = player;
        wildPal = wild;
        playerPal.HP = playerPal.MaxHP;
        wildPal.HP = wildPal.MaxHP;
        isBattleActive = true;
        // No UI/UX output here. All battle info will be printed after the standard battle intro in the command handler.
    }

    private static bool playerDefending = false;
    private static bool enemyDefending = false;

    public static void HandlePlayerAction(string action)
    {
        if (!isBattleActive || playerPal == null || wildPal == null) return;
        switch (action)
        {
            case "basic":
                DoAttack(playerPal, wildPal, playerPal.Moves?[0] ?? "Basic Attack");
                break;
            case "special":
                DoAttack(playerPal, wildPal, playerPal.Moves?.Count > 1 ? playerPal.Moves[1] : "Special Attack", isSpecial:true);
                break;
            case "defend":
                playerDefending = true;
                int heal = 5;
                playerPal.HP = Math.Min(playerPal.MaxHP, playerPal.HP + heal);
                Typewriter.TypeLine($"{playerPal.Name} braces for the next attack and heals for {heal} HP!");
                Typewriter.TypeLine($"{playerPal.Name} HP: {playerPal.HP}/{playerPal.MaxHP} | {wildPal.Name} HP: {wildPal.HP}/{wildPal.MaxHP}");
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
                    return; // Do not skip player's turn if no potion, let them try again
                }
                break;
            case "tame":
                TryTame(wildPal);
                break;
            case "run":
                Typewriter.TypeLine("You ran away!");
                EndBattle();
                States.ChangeState(StateTypes.Exploring);
                return;
        }
        if (wildPal != null && wildPal.HP > 0 && isBattleActive)
            EnemyTurn();
        CheckBattleEnd();
    }

    private static void DoAttack(Pal attacker, Pal defender, string move, bool isSpecial = false, bool halveDamage = false)
    {
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
        // Print HP of both pals after the attack
        if (playerPal != null && wildPal != null)
        {
            Typewriter.TypeLine($"{playerPal.Name} HP: {playerPal.HP}/{playerPal.MaxHP} | {wildPal.Name} HP: {wildPal.HP}/{wildPal.MaxHP}");
        }
    }

    private static void HealPal(Pal pal)
    {
        int heal = 15;
        pal.HP = Math.Min(pal.MaxHP, pal.HP + heal);
        Typewriter.TypeLine($"{pal.Name} healed for {heal} HP!");
    }

    private static void TryTame(Pal wild)
    {
        if (wild == null)
        {
            Typewriter.TypeLine("There is no wild Pal to tame.");
            return;
        }
        int chance = 30 + (100 * (wild.MaxHP - wild.HP) / wild.MaxHP);
        int roll = rng.Next(0, 100);
        if (roll < chance)
        {
            Typewriter.TypeLine($"You tamed {wild.Name}! It joins your team.");
            Player.AddPal(wild);
            Player.CurrentLocation.RemovePal(wild);
            EndBattle();
            Console.Clear();
            States.ChangeState(StateTypes.Exploring);
            Player.Look();
        }
        else
        {
            Typewriter.TypeLine($"Taming failed!");
        }
    }

    private static void EnemyTurn()
    {
        if (wildPal == null || playerPal == null) return;
        // 0: basic, 1: special, 2: defend
        int action = rng.Next(0, 3);
        if (action == 2) // defend
        {
            // Enemy braces and heals
            int heal = 5;
            wildPal.HP = Math.Min(wildPal.MaxHP, wildPal.HP + heal);
            Typewriter.TypeLine($"{wildPal.Name} braces for the next attack and heals for {heal} HP!");
            Typewriter.TypeLine($"{playerPal.Name} HP: {playerPal.HP}/{playerPal.MaxHP} | {wildPal.Name} HP: {wildPal.HP}/{wildPal.MaxHP}");
            // Set a flag so next player attack is halved
            enemyDefending = true;
        }
        else
        {
            bool useSpecial = (action == 1);
            string move = useSpecial && wildPal.Moves?.Count > 1 ? wildPal.Moves[1] : wildPal.Moves?[0] ?? "Wild Attack";
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

    private static void CheckBattleEnd()
    {
        if (playerPal == null || wildPal == null) return;
        if (playerPal.HP <= 0)
        {
            Typewriter.TypeLine($"{playerPal.Name} fainted! You lost the battle.");
            EndBattle();
            States.ChangeState(StateTypes.Exploring);
        }
        else if (wildPal.HP <= 0)
        {
            Typewriter.TypeLine($"{wildPal.Name} fainted! You won the battle!");
            EndBattle();
            States.ChangeState(StateTypes.Exploring);
        }
    }

    private static void EndBattle()
    {
        isBattleActive = false;
        playerPal = null;
        wildPal = null;
    }
}
