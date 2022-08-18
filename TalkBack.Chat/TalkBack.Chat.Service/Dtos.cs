namespace TalkBack.Chat.Service.Dtos
{

    public record MessageDto(/*Guid Id,*/ string Sender, /*string[] Recievers,*/ string Content, DateTimeOffset TimeStamp);

    public record ConversationDto(Guid Id, string GroupName);

    public record CreateMessageDto(string ConversationId, string Content);
}