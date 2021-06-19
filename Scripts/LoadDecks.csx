#r "nuget: Newtonsoft.Json"

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LoadDeckInformation
{
    class Program
    {
        static void Main(string[] args)
        {
            string myJsonResponse = System.IO.File.ReadAllText(@"C:\tests\pokemon-tcg-data\decks\en\base1.json");

            var myDeserializedClass = JsonConvert.DeserializeObject<Root[]>(myJsonResponse);

            using (var writer = new StreamWriter("out.csv"))
            {
                writer.WriteLine("deck_id|deck_name|deck_res|card_id");
                foreach (var deck in myDeserializedClass)
                {
                    string deckId = deck.id;
                    string deckName = deck.name;
                    string deckTypes = JsonConvert.SerializeObject(deck.types);

                    foreach (var card in deck.cards)
                    {
                        for (int i = 0; i < card.count; i++)
                            writer.WriteLine("{0}|{1}|{2}|{3}", deckId, deckName, deckTypes, card.id);
                    }
                }
            }


        }
    }

    public class Card
    {
        public string id { get; set; }
        public string name { get; set; }
        public string rarity { get; set; }
        public int count { get; set; }
    }

    public class Root
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<string> types { get; set; }
        public List<Card> cards { get; set; }
    }
}