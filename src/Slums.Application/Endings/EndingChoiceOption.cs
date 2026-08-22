using Slums.Core.Endings;

namespace Slums.Application.Endings;

public sealed record EndingChoiceOption(EndingId Id, string Label, string Requirements);
