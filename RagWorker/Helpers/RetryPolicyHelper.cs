namespace RagWorker.Helpers;

public static class RetryPolicyHelper
{
    public static async Task ExecuteAsync(
        Func<Task> action,
        int maxRetries = 3,
        int delayMilliseconds = 500)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                attempt++;
                await action();
                return;
            }
            catch when (attempt < maxRetries)
            {
                await Task.Delay(delayMilliseconds);
            }
        }
    }

    public static async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        int maxRetries = 3,
        int delayMilliseconds = 500)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                attempt++;
                return await action();
            }
            catch when (attempt < maxRetries)
            {
                await Task.Delay(delayMilliseconds);
            }
        }
    }
}