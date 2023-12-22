using Xunit;
using Xunit.Abstractions;

namespace AoC23;

public class Day20
{
    private readonly ITestOutputHelper _testOutputHelper;

    public Day20(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public class Module(string Name, params string[] TargetNames)
    {
        public string Name { get; } = Name;
        public string[] TargetNames { get; } = TargetNames;

        public virtual void RegisterAsInput(string InputName) { }
        public virtual Pulse[] Pulse(Pulse input)
        {
            return TargetNames.Select(x => new Pulse(x, Name, false)).ToArray();
        }
    }

    public class FlipFlopModule(string Name, params string[] TargetNames) : Module(Name, TargetNames)
    {
        bool State { get; set; } = false;

        public override Pulse[] Pulse(Pulse input)
        {
            if (input.High)
                return Array.Empty<Pulse>();

            State = !State;
            return TargetNames.Select(x => new Pulse(x, Name, State)).ToArray();
        }
    }

    public class ConjunctionModule(string Name, params string[] TargetNames) : Module(Name, TargetNames)
    {
        private readonly Dictionary<string, bool> _latestInputs = [];

        public override Pulse[] Pulse(Pulse input)
        {
            _latestInputs[input.SourceModuleName] = input.High;
            var pulseToSend = !_latestInputs.Values.All(x => x);
            return TargetNames.Select(x => new Pulse(x, Name, pulseToSend)).ToArray();
        }

        public override void RegisterAsInput(string InputName) => _latestInputs.Add(InputName, false);
    }

    public record Pulse(string TargetModuleName, string SourceModuleName, bool High);

    [Fact]
    public void Task1()
    {
        var modules = ReadInputs();
        foreach (var module in modules.Values)
        {
            foreach (var targetModule in module.TargetNames)
            {
                if (modules.TryGetValue(targetModule, out Module value))
                    value.RegisterAsInput(module.Name);
            }
        }

        var countHighPulses = 0;
        var countLowPulses = 0;

        var pulseQueue = new Queue<Pulse>();
        for (int i = 0; i < 1000; i++)
            pulseQueue.Enqueue(new Pulse("broadcaster", "button", false));
        while (pulseQueue.TryDequeue(out var pulse))
        {
            if (pulse.High)
                countHighPulses++;
            else
                countLowPulses++;

            if (!modules.ContainsKey(pulse.TargetModuleName))
                continue;
            var newPulses = modules[pulse.TargetModuleName].Pulse(pulse);
            foreach (var newPulse in newPulses)
            {
                pulseQueue.Enqueue(newPulse);
            }
        }

        _testOutputHelper.WriteLine((countHighPulses * countLowPulses).ToString());
    }

    [Fact]
    public void Task2()
    {
        var modules = ReadInputs();
        foreach (var module in modules.Values)
        {
            foreach (var targetModule in module.TargetNames)
            {
                if (modules.TryGetValue(targetModule, out Module value))
                    value.RegisterAsInput(module.Name);
            }
        }
        int buttonPresses;
        for (buttonPresses = 1; ; buttonPresses++)
        {
            var pulseQueue = new Queue<Pulse>();
            pulseQueue.Enqueue(new Pulse("broadcaster", "button", false));
            while (pulseQueue.TryDequeue(out var pulse))
            {
                if (pulse.TargetModuleName == "rx" && !pulse.High)
                {
                    _testOutputHelper.WriteLine(buttonPresses.ToString());
                    return;
                }
                if (!modules.ContainsKey(pulse.TargetModuleName))
                    continue;
                var newPulses = modules[pulse.TargetModuleName].Pulse(pulse);
                foreach (var newPulse in newPulses)
                {
                    pulseQueue.Enqueue(newPulse);
                }
            }
        }

    }

    private Dictionary<string, Module> ReadInputs()
    {
        var lines = File.ReadAllLines("Inputs/Day20.txt");
        var modules = new List<Module>();

        foreach (var line in lines)
        {
            var splitted = line.Split("->", StringSplitOptions.TrimEntries);
            var targets = splitted[1].Split(',', StringSplitOptions.TrimEntries);
            if (splitted[0] == "broadcaster")
            {
                modules.Add(new Module(splitted[0], targets));
            }
            else if (splitted[0][0] == '%')
            {
                modules.Add(new FlipFlopModule(splitted[0][1..], targets));
            }
            else if (splitted[0][0] == '&')
            {
                modules.Add(new ConjunctionModule(splitted[0][1..], targets));
            }
            else throw new NotImplementedException();
        }

        return modules.ToDictionary(x => x.Name, x => x);
    }
}
