using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Skills {
  /// All of the known skills.
  public static List<Skill> m_all;
  public static List<Skill> all {
    get {
        if (m_all == null)
        {
            m_all = new List<Skill>(){
            // Disciplines.
                new BattleHardening(),
                new DualWield(),

                // Masteries.
                new Archery(),
                new AxeMastery(),
                new ClubMastery(),
                new SpearMastery(),
                Swordfighting(),
                WhipMastery(),

                // Slays.
                SlayDiscipline("Animals", "animal"),
                SlayDiscipline("Bugs", "bug"),
                SlayDiscipline("Dragons", "dragon"),
                SlayDiscipline("Fae Folk", "fae"),
                SlayDiscipline("Goblins", "goblin"),
                SlayDiscipline("Humans", "human"),
                SlayDiscipline("Jellies", "jelly"),
                SlayDiscipline("Kobolds", "kobold"),
                SlayDiscipline("Plants", "plant"),
                SlayDiscipline("Saurians", "saurian"),
                SlayDiscipline("Undead", "undead"),

                // Spells.
                // Divination.
                SenseItems(),

                // Conjuring.
                Flee(),
                Escape(),
                Disappear(),

                // Sorcery.
                Icicle(),
                BrilliantBeam(),
                Windstorm(),
                FireBarrier(),
                TidalWave(),
            };
        }
        return m_all;
    }
  }

    static Dictionary<string, Skill> m_byName;
  public static Dictionary<string, Skill> _byName {
    get {
        if (m_byName == null)
        {
            m_byName = new Dictionary<string, Skill>();
            foreach (var skill in all) 
                m_byName.Add(skill.name, skill);
        }
        return m_byName;
    }
  }

  public static Skill find(string name) {
    var skill = _byName[name];
    if (skill == null) throw new System.Exception($"Unknown skill '{name}'.");
    return skill;
  }
}
