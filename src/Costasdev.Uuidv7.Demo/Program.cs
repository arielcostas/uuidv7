using Costasdev.Uuidv7;

Console.WriteLine("How many UUIDs do you want to generate?");
var count = int.Parse(Console.ReadLine());

for (int i = 0; i < count; i++)
{
	Console.WriteLine(UuidGenerator.PrettyGenerate());
	Task.Delay(1000).Wait();
}
