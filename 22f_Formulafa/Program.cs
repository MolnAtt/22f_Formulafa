using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _22f_Formulafa
{
	internal class Program
	{
		class Formula
		{
			string jel; // pl.: →, ∧, p, q, ...
			List<Formula> gyerekei; 

			public Formula(string jel, List<Formula> gyerekei)
			{
				this.jel = jel;
				this.gyerekei = gyerekei;
			}

			//public Formula(string jel)
			//{
			//	this.jel = jel;
			//	this.gyerekei = new List<Formula>();
			//}

			public Formula(string jel) : this(jel, new List<Formula>()) { }
			public static Formula operator +(Formula a, Formula b) => new Formula("V",new List<Formula> { a, b });
			public static Formula operator *(Formula a, Formula b) => new Formula("&", new List<Formula> { a, b });
			public static Formula operator !(Formula a) => new Formula("-", new List<Formula> { a });
			public Formula Akkor(Formula that) => new Formula("→", new List<Formula> { this, that });
			public Formula Csakkor(Formula that) => new Formula("↔", new List<Formula> { this, that });
			static int futo_id = 0 ;
			public string Diagnosztika()
			{
				string graphviz_code = $"\t\"{ futo_id }\" [label=\"{this.jel}\"];\n";
				int szuloid = futo_id;
				foreach (Formula gyerek in gyerekei)
				{
					graphviz_code += $"\t\"{szuloid}\" -> \"{++futo_id}\";\n";
					graphviz_code += gyerek.Diagnosztika();
				}
				return graphviz_code;
			}
			public override string ToString()
			{
				switch (gyerekei.Count)
				{
					case 0:
						return jel;
					case 1: 
						return $"{ jel } { gyerekei[0]}";
					case 2:
						return $"({gyerekei[0]} {jel} {gyerekei[1]})";
					default:
						return jel + "("+String.Join(", ", gyerekei)+")";
				}
			}

			public static Formula Parse(string formulastring)
			{
				Stack<string> input = TolatóUdvar(formulastring);

				Stack<Formula> formulaverem = new Stack<Formula>();
				foreach (string s in input.Reverse())
				{
					if (Atomi_formula(s))
						formulaverem.Push(new Formula(s)); // nem az igazi... mert cím szerint nem azonosak az ugyanolyan feliratú betűk...
					else
						switch (s)
						{
							case "&":
								formulaverem.Push(formulaverem.Pop() * formulaverem.Pop());
								break;
							case "V":
								formulaverem.Push(formulaverem.Pop() + formulaverem.Pop());
								break;
							case "→":
								formulaverem.Push(formulaverem.Pop().Akkor(formulaverem.Pop()));
								break;
							case "↔":
								formulaverem.Push(formulaverem.Pop().Csakkor(formulaverem.Pop()));
								break;
							case "-":
								formulaverem.Push(!formulaverem.Pop());
								break;
						}
				}
				return formulaverem.Peek();
			}
		}

		static Dictionary<string, int> prioritás = new Dictionary<string, int>
		{
			 {"-", 5 },
			 {"&", 4 },
			 {"V", 3 },
			 {"→", 2 },
			 {"↔", 1 },
		};
		static Stack<string> TolatóUdvar(string vonat)
		{
			Stack<string> output = new Stack<string>();
			Stack<char> operator_stack = new Stack<char>();

			vonat = Ha_nincs_körülötte_zárójel_akkor_rakunk_köré_egyet(vonat);

			int i = 0;
			string adat = "";
			while (i < vonat.Length)
			{
				(adat, i) = Beolvas(vonat, i);

				if (Atomi_formula(adat))
					output.Push(adat);
				else if (adat == "(")
					operator_stack.Push(adat[0]);
				else if (Művelet(adat))
				{
					Alacsonyabb_precedenciájúig_vagy_nyitójelig_átpakol(operator_stack, output, adat);
					operator_stack.Push(adat[0]);
				}
				else if (adat == ")")
				{
					Nyitózárójelig_átpakol(operator_stack, output);
					operator_stack.Pop(); // kidobjuk a nyitó zárójelet
				}
				Diagnosztika(output, operator_stack);
			}
			return output;
		}
		private static void Nyitózárójelig_átpakol(Stack<char> operator_stack, Stack<string> output)
		{
			while (operator_stack.Peek() != '(')
				output.Push(operator_stack.Pop().ToString());
		}
		private static void Alacsonyabb_precedenciájúig_vagy_nyitójelig_átpakol(Stack<char> operator_stack, Stack<string> output, string adat)
		{
			while (operator_stack.Count > 0 && operator_stack.Peek() != '(' && prioritás[adat] <= prioritás[operator_stack.Peek().ToString()])
				output.Push(operator_stack.Pop().ToString());
		}
		public static void Diagnosztika(Stack<string> output, Stack<char> operator_stack)
		{
			Console.Write("output = [ ");
			foreach (string adat in output.Reverse())
			{
				Console.Write(adat + " ");
			}
			Console.WriteLine("]");

			Console.Write("operator_stack = [ ");
			foreach (char op in operator_stack.Reverse())
			{
				Console.Write(op + " ");
			}
			Console.WriteLine("]");
			Console.WriteLine("--------------------------------------------");
		}
		private static bool Művelet(string adat) => prioritás.ContainsKey(adat);
		private static bool Atomi_formula(string adat) => Atomi_formula((char)adat[0]);
		private static (string, int) Beolvas(string vonat, int honnan)
		{
			if (!Atomi_formula(vonat[honnan]))
				return (vonat[honnan].ToString(), honnan + 1);
			int i = honnan;
			while (Atomi_formula(vonat[i]))
				i++;
			return (vonat.Substring(honnan, i - honnan), i);
		}
		private static bool Atomi_formula(char v) => "qwertzuiopasdfghjklyxcvbnmQWERTZUIOPASDFGHJKLYXCVBNM".Contains(v);




		/// <summary>
		/// Megszámolja a zárójelmélységet: "(" a jel +1-et jelent, a ")" -1-et jelent, és ahogy adja ezeket össze, úgy 0-át kap, de úgy, hogy az összeadogatás során egyszer sem jött ki -1. Ha 0-át kapott, az azt jelenti, hogy jó benne a zárójelezés. Ha emellett közbülső helyen volt 0, 
		/// </summary>
		/// <param name="vonat"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		private static string Ha_nincs_körülötte_zárójel_akkor_rakunk_köré_egyet(string vonat)
		{
			int melyseg = 0;
			int db_0 = 0;
			foreach (char c in vonat)
			{
				switch (c)
				{
					case '(':
						melyseg++;
						break;
					case ')':
						melyseg--;
						break;
				}
				if (melyseg < 0)
					throw new Exception("Hibás zárójelezés! (túl sok csukó zárójel)");
				if (melyseg == 0)
					db_0++;
			}
			if (melyseg > 0)
				throw new Exception("Hibás zárójelezés! (túl sok nyitó zárójel)");
			return 1 == db_0 ? vonat : "(" + vonat + ")";
		}



		static void Main(string[] args)
		{
			Formula p = new Formula("p");
			Formula q = new Formula("q");
			Formula s = new Formula("s");
			Formula p_vagy_q = p + q;
			Formula A = ((p * q).Akkor(!p)).Csakkor(s.Akkor(!p + q));
			Console.WriteLine(A.Diagnosztika());
			Console.WriteLine(A);
			Formula B = Formula.Parse("(((p&q)→-p)↔(s→(-pVq)))");
            Console.WriteLine(B.Diagnosztika()); ;

        }
	}
}
