using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Versagen.PlayerSystem;

namespace Versagen.Entity
{
    public class stats : IStat
    {
        public stats(int HP, int atk, int def, int spd)
        {
            health = HP;
            attack = atk;
            defense = def;
            speed = spd;
        }

        public static int[] playerStats = new int[] {10, 5, 3, 8};

        string name { get; }
        int health { get; }
        int attack { get; }
        int defense { get; }
        int speed { get; }
        public object enemy { get; set; }

        public int currentAtk(int attack, int health)
        {
            int currentAttack = attack.CompareTo(defense);
            if (attack > defense)
            {
                currentAttack += attack;
                health -= attack;
            }
            if (attack < defense)
            {
                health -= attack;
            }
            return currentAttack;
        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
        /*


bool equals(T value)
{
   if(player.speed == enemy.speed)
   {
       Random r = new Random();
       int luck = r.Next(1, 10);
       if(luck<=5) { return true; }
   }
   return false;
} */
        /*
        public int CompareTo(String player, String enemy, int health, int attack, int defense, int speed)
        {
            if (player.(speed) > enemy.(speed))
            {
                player(attack).enemy(health);
                enemy(attack).player(health);
            }
            if (player.(speed) < enemy.(speed))
            {
                enemy(attack).player(health);
                player(attack).enemy(health);
            }
            else
            {
                player(speed).equals(enemy(speed));
            }
            return speed;
        } */
        public string Name { get; }
        public string Modifies { get; }
    }
}