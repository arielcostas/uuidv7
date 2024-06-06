using Costasdev.Uuidv7;

Console.WriteLine("How many UUIDs do you want to generate?");
var count = int.Parse(Console.ReadLine() ?? "0");

for (int i = 0; i < count; i++)
{
	var uuid = Uuid7.NewUuid();
	Console.WriteLine(uuid.ToString());
	var parsed = Uuid7.TryParse(uuid.ToString(), out var result);
	Console.WriteLine(parsed ? "Parsed successfully" : "Failed to parse");
	Console.WriteLine("Parsed UUID: " + result);
	Task.Delay(1000).Wait();
}
