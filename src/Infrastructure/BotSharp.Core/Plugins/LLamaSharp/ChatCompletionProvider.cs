using BotSharp.Abstraction.Agents.Models;
using BotSharp.Abstraction.Conversations.Models;
using BotSharp.Abstraction.MLTasks;
using LLama;
using System.IO;

namespace BotSharp.Core.Plugins.LLamaSharp;

public class ChatCompletionProvider : IChatCompletion
{
    private readonly IChatModel _model;
    private readonly LlamaSharpSettings _settings;

    public ChatCompletionProvider(LlamaSharpSettings settings)
    {
        _settings = settings;
        _model = new LLamaModel(new LLamaParams(model: _settings.ModelPath,
            n_ctx: _settings.MaxContextLength,
            interactive: _settings.Interactive,
            repeat_penalty: _settings.RepeatPenalty,
            verbose_prompt: _settings.VerbosePrompt,
            n_gpu_layers: _settings.NumberOfGpuLayer));

        var prompt = GetInstruction();
        _model.InitChatPrompt(prompt, "UTF-8");
        _model.InitChatAntiprompt(new string[] { "user:" });
    }

    public int Priority => 100;

    public async Task GetChatCompletionsAsync(List<RoleDialogModel> conversations,
        Func<string, Task> onChunkReceived)
    {
        string totalResponse = "";
        var prompt = GetInstruction();
        var content = string.Join("\n ", conversations.Select(x => $"{x.Role}: {x.Text.Replace("user:", "")}")).Trim();
        content += "\n assistant: ";
        foreach (var response in _model.Chat(content, prompt, "UTF-8"))
        {
            Console.Write(response);
            totalResponse += response;
            await onChunkReceived(response);
        }

        Console.WriteLine();
    }

    public Task<string> GetChatCompletionsAsync(Agent agent, List<RoleDialogModel> conversations)
    {
        string totalResponse = "";
        var prompt = GetInstruction();
        var content = string.Join("\n ", conversations.Select(x => $"{x.Role}: {x.Text.Replace("user:", "")}")).Trim();
        content += "\n assistant: ";
        foreach (var response in _model.Chat(content, prompt, "UTF-8"))
        {
            Console.Write(response);
            totalResponse += response;
        }

        return Task.FromResult(totalResponse);
    }

    public List<RoleDialogModel> GetChatSamples()
    {
        var samples = new List<RoleDialogModel>();
        if (!string.IsNullOrEmpty(_settings.ChatSampleFile))
        {
            var lines = File.ReadAllLines(_settings.ChatSampleFile);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var role = line.Substring(0, line.IndexOf(' ') - 1);
                var content = line.Substring(line.IndexOf(' ') + 1);

                samples.Add(new RoleDialogModel
                {
                    Role = role,
                    Text = content
                });
            }
        }
        return samples;
    }

    public string GetInstruction()
    {
        var instruction = "";
        if (!string.IsNullOrEmpty(_settings.InstructionFile))
        {
            instruction = File.ReadAllText(_settings.InstructionFile);
        }

        instruction += "\n";
        foreach (var message in GetChatSamples())
        {
            instruction += $"\n{message.Role}: {message.Text}";
        }

        return instruction;
    }
}