namespace AdventureS25;

using System;
using System.Collections.Generic;

public class Pal
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string? InitialDescription { get; set; }
    public bool IsAcquirable { get; set; }
    public string? AsciiArt { get; set; }
    public string? Location { get; set; }
    public List<string> Moves { get; set; }

    public int HP { get; set; }
    public int MaxHP { get; set; }

    public int Level { get; set; }
    public int ExperiencePoints { get; private set; }
    public int ExperienceToNextLevel { get; private set; }

    public int MaxBasicAttackUses { get; set; }
    public int BasicAttackUses { get; set; }
    public int MaxSpecialAttackUses { get; set; }
    public int SpecialAttackUses { get; set; }

    public int BaseAttackDamage { get; set; }
    public int BaseSpecialAttackDamage { get; set; }

    public Pal(string name, string description, string? initialDescription, bool isAcquirable, string? asciiArt, string? location, List<string> moves, int maxHP = 50)
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

        // Base damage stats
        BaseAttackDamage = 10; // Default base attack damage
        BaseSpecialAttackDamage = 15; // Default base special attack damage
    }

    public void AddExperience(int amount, bool suppressMessage = false)
    {
        if (amount <= 0) return;

        ExperiencePoints += amount;
        if (!suppressMessage)
        {
            Typewriter.TypeLine($"{Name} gained {amount} XP!");
        }

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

        // Increase damage stats
        int attackIncrease = 2;
        BaseAttackDamage += attackIncrease;

        int specialAttackIncrease = 3;
        BaseSpecialAttackDamage += specialAttackIncrease;

        ExperiencePoints -= ExperienceToNextLevel; // Subtract XP used for this level
        ExperienceToNextLevel = Level * 100; // Example: Next level requires Level * 100 XP
                                             // Or use a more scaling formula like (int)(ExperienceToNextLevel * 1.25) for a 25% increase

        // If XP still exceeds new threshold (e.g., gained multiple levels at once)
        // The while loop in AddExperience will handle calling LevelUp again.
    }

    public void InitializeStatsForLevel()
    {
        if (this.Level <= 0) this.Level = 1;

        if (this.Level == 1)
        {
            this.ExperiencePoints = 0;
            this.ExperienceToNextLevel = 100; // Base XP for Lvl 1 to reach Lvl 2
            this.HP = this.MaxHP;
            return;
        }

        // Reset to Level 1 base stats before calculation.
        // The constructor sets MaxHP, BaseAttackDamage, BaseSpecialAttackDamage to Lvl 1 values.
        // These are assumed to be the true Lvl 1 base stats.

        int simulatedCurrentLevel = 1;
        // ExperiencePoints for Lvl 1 is 0, ExperienceToNextLevel for Lvl 1 to reach Lvl 2 is 100.
        // These are set by the constructor and don't need to be reset if Level > 1 initially.
        // However, to be safe and clear, especially if this method could be called in other contexts:
        this.ExperiencePoints = 0;
        this.ExperienceToNextLevel = 100; // XP to reach Lvl 2

        // Re-fetch Lvl 1 stats if they could have been modified from constructor defaults.
        // This part is tricky without knowing the exact design of how base stats are stored vs modified.
        // For now, we assume the current MaxHP, BaseAttackDamage, etc., ARE the Lvl 1 base values
        // if this method is called right after deserialization and before any other modifications.
        // If not, one would typically fetch base stats from a definition or reset them to known Lvl 1 constants.
        // Let's proceed assuming current values are Lvl 1 base if Level was >1 from JSON.

        while (simulatedCurrentLevel < this.Level)
        {
            simulatedCurrentLevel++;

            // Stat increases (must match LevelUp() method's increments)
            int hpIncrease = 10;
            this.MaxHP += hpIncrease;

            int attackIncrease = 2;
            this.BaseAttackDamage += attackIncrease;

            int specialAttackIncrease = 3;
            this.BaseSpecialAttackDamage += specialAttackIncrease;

            // Update ExperienceToNextLevel for the *new* simulatedCurrentLevel
            this.ExperienceToNextLevel = simulatedCurrentLevel * 100;
        }

        this.ExperiencePoints = 0; // Ensure XP is 0 at the target level
        this.HP = this.MaxHP;      // Heal to new max HP
    }

    public string GetLocationDescription()
    {
        return $"{AsciiArt}\n{InitialDescription}";
    }

    public Pal Clone()
    {
        // MemberwiseClone performs a shallow copy. If Pal contains complex reference types
        // that should be unique per instance (e.g., a list of status effects that can change),
        // a more detailed deep copy would be needed for those members.
        return (Pal)this.MemberwiseClone();
    }

    public void ResetAttackUses()
    {
        BasicAttackUses = MaxBasicAttackUses;
        SpecialAttackUses = MaxSpecialAttackUses;
    }
}
