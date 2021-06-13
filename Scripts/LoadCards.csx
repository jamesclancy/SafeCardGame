#r "nuget: Newtonsoft.Json"

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoadCards
{
    class Program
    {
        static void Main(string[] args)
        {
            var myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(File.ReadAllText(@"C:\tests\pokemon-tcg-data\cards\en\base1.json"));

            using (var writer = new StreamWriter("values.txt"))
            {
                writer.WriteLine("card_id	card_name	card_description	card_image_url	card_thumbnail_image_url	card_primary_resource	card_type	card_enter_special_effects	card_exit_special_effects	card_creature_health	card_creature_weaknesses	card_creature_attacks	card_resources_available_on_first_turn	card_resources_added");

                foreach (var poke in myDeserializedClass)
                {
                    var csvLine = string.Format("{0}	{1}	{2}	{3}	{4}	{5}	{6}	{7}	{8}	{9}	{10}	{11}	{12}	{13}",

                            poke.id,
        poke.name,
        poke.flavorText,
        poke.images.large,
        poke.images.small,
        poke.types?.FirstOrDefault() ?? string.Empty,
        GetTranslatedType(poke.supertype),
        null,
        null,
        string.IsNullOrWhiteSpace(poke.hp) ? null : poke.hp,
       poke.weaknesses == null? "[]": JsonConvert.SerializeObject(poke.weaknesses.Select(x=>x.type).Distinct()),
          poke.attacks == null ? null : JsonConvert.SerializeObject(poke.attacks.Select(x => {
              int v;

             if(!int.TryParse(x.damage, out v))
              {
                  v = 0;
              }
              return new AttackDto
              {
                  Name = x.name,
                  Damage = v,
                  Cost = x.cost.GroupBy(y => y)
                                 .Select(y => new { y.Key, Count = y.Count() })
                                 .ToDictionary(y => y.Key, y => y.Count)
              };
          })),
        false,
        false
        );
                    writer.WriteLine(csvLine);
                    Console.WriteLine(poke.name);
                }
            }
        }


        public static string GetTranslatedType(string jsonType)
        {
            switch (jsonType)
            {
                case "Pokémon":
                    return "Character";
                case "Trainer":
                    return "Effect";
                case "Energy":
                    return "Resource";
                default:
                    throw new NotImplementedException();
            }
        }
    }



    public class AttackDto
    {
        public int Damage { get; set; }
        public string Name { get; set; }
        public Dictionary<string, int> Cost { get; set; }
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Ability
    {
        public string name { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Attack
    {
        public string name { get; set; }
        public List<string> cost { get; set; }
        public int convertedEnergyCost { get; set; }
        public string damage { get; set; }
        public string text { get; set; }
    }

    public class Weakness
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Legalities
    {
        public string unlimited { get; set; }
    }

    public class Images
    {
        public string small { get; set; }
        public string large { get; set; }
    }

    public class Resistance
    {
        public string type { get; set; }
        public string value { get; set; }
    }

    public class Root
    {
        public string id { get; set; }
        public string name { get; set; }
        public string supertype { get; set; }
        public List<string> subtypes { get; set; }
        public string level { get; set; }
        public string hp { get; set; }
        public List<string> types { get; set; }
        public string evolvesFrom { get; set; }
        public List<Ability> abilities { get; set; }
        public List<Attack> attacks { get; set; }
        public List<Weakness> weaknesses { get; set; }
        public List<string> retreatCost { get; set; }
        public int convertedRetreatCost { get; set; }
        public string number { get; set; }
        public string artist { get; set; }
        public string rarity { get; set; }
        public string flavorText { get; set; }
        public List<int> nationalPokedexNumbers { get; set; }
        public Legalities legalities { get; set; }
        public Images images { get; set; }
        public List<string> evolvesTo { get; set; }
        public List<Resistance> resistances { get; set; }
        public List<string> rules { get; set; }
    }


}
