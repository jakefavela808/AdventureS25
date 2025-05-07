namespace AdventureS25;

using System;
using System.Collections.Generic;

public class Pal
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string InitialDescription { get; set; }
    public bool IsAcquirable { get; set; }
    public string AsciiArt { get; set; }
    public string Location { get; set; }
    public List<string> Moves { get; set; }

    public int HP { get; set; }
    public int MaxHP { get; set; }

    public int Level { get; private set; }
    public int ExperiencePoints { get; private set; }
    public int ExperienceToNextLevel { get; private set; }

    public int MaxBasicAttackUses { get; set; }
    public int BasicAttackUses { get; set; }
    public int MaxSpecialAttackUses { get; set; }
    public int SpecialAttackUses { get; set; }

    public Pal(string name, string description, string initialDescription, bool isAcquirable, string asciiArt, string location, List<string> moves, int maxHP = 50)
    {
        Name = name;
        Description = description;
        InitialDescription = initialDescription;
        IsAcquirable = isAcquirable;
        AsciiArt = asciiArt;
        Location = location;
        Moves = moves;
        MaxHP = maxHP;
        HP = maxHP;

        Level = 1;
        ExperiencePoints = 0;
        ExperienceToNextLevel = 100; // Initial XP for next level

        MaxBasicAttackUses = 15; // Default max basic attack uses
        BasicAttackUses = MaxBasicAttackUses;
        MaxSpecialAttackUses = 5;  // Default max special attack uses
        SpecialAttackUses = MaxSpecialAttackUses;
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;

        ExperiencePoints += amount;
        Typewriter.TypeLine($"{Name} gained {amount} XP!");

        while (ExperiencePoints >= ExperienceToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        Typewriter.TypeLine($"{Name} grew to Level {Level}!");

        // Stat increases
        int hpIncrease = 10; // Example: Increase MaxHP by 10 each level
        MaxHP += hpIncrease;
        HP = MaxHP; // Heal to new max HP
        Typewriter.TypeLine($"{Name}'s Max HP increased by {hpIncrease}!");

        ExperiencePoints -= ExperienceToNextLevel; // Subtract XP used for this level
        ExperienceToNextLevel = Level * 100; // Example: Next level requires Level * 100 XP
                                             // Or use a more scaling formula like (int)(ExperienceToNextLevel * 1.25) for a 25% increase

        // If XP still exceeds new threshold (e.g., gained multiple levels at once)
        // The while loop in AddExperience will handle calling LevelUp again.
    }

    public string GetLocationDescription()
    {
        return $"{AsciiArt}\n{InitialDescription}";
    }
}
