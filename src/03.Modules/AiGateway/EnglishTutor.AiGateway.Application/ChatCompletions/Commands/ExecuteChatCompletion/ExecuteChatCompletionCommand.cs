using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.AiGateway.Contracts.Dtos;

namespace EnglishTutor.AiGateway.Application.ChatCompletions.Commands.ExecuteChatCompletion;

public sealed record ExecuteChatCompletionCommand(
    ExecuteChatCompletionRequest Request) : ICommand<ExecuteChatCompletionResult>;
