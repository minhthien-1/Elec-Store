namespace ElectronicsStore.API.Commands
{
    public class CommandResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        public static CommandResult Success(string msg = "Thành công!") => new CommandResult { IsSuccess = true, Message = msg };
        public static CommandResult Fail(string msg) => new CommandResult { IsSuccess = false, Message = msg };
    }
}
