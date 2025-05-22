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
        ExperienceToNextLevel = 100;
        MaxBasicAttackUses = 15;
        BasicAttackUses = MaxBasicAttackUses;
        MaxSpecialAttackUses = 5;
        SpecialAttackUses = MaxSpecialAttackUses;
        BaseAttackDamage = 10;
        BaseSpecialAttackDamage = 15;
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

        int hpIncrease = 10;
        MaxHP += hpIncrease;
        HP = MaxHP;

        int attackIncrease = 2;
        BaseAttackDamage += attackIncrease;

        int specialAttackIncrease = 3;
        BaseSpecialAttackDamage += specialAttackIncrease;

        ExperiencePoints -= ExperienceToNextLevel;
        ExperienceToNextLevel = Level * 100;
    }

    public void InitializeStatsForLevel()
    {
        if (this.Level <= 0) this.Level = 1;

        if (this.Level == 1)
        {
            this.ExperiencePoints = 0;
            this.ExperienceToNextLevel = 100;
            this.HP = this.MaxHP;
            return;
        }

        int simulatedCurrentLevel = 1;
        this.ExperiencePoints = 0;
        this.ExperienceToNextLevel = 100;
        while (simulatedCurrentLevel < this.Level)
        {
            simulatedCurrentLevel++;

            int hpIncrease = 10;
            this.MaxHP += hpIncrease;

            int attackIncrease = 2;
            this.BaseAttackDamage += attackIncrease;

            int specialAttackIncrease = 3;
            this.BaseSpecialAttackDamage += specialAttackIncrease;

            this.ExperienceToNextLevel = simulatedCurrentLevel * 100;
        }

        this.ExperiencePoints = 0;
        this.HP = this.MaxHP;
    }

    public string GetLocationDescription()
    {
        return $"{AsciiArt}\n{InitialDescription}";
    }

    public Pal Clone()
    {
        return (Pal)this.MemberwiseClone();
    }

    public void ResetAttackUses()
    {
        BasicAttackUses = MaxBasicAttackUses;
        SpecialAttackUses = MaxSpecialAttackUses;
    }
}
