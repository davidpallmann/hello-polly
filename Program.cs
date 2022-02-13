using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using System.IO;

RegionEndpoint region = RegionEndpoint.USWest2;

if (args.Length >= 3)
{
    var pollyClient = new AmazonPollyClient(region);

    var languageCode = args[0];
    var voiceId = args[1];
    var text =   args[2];

    if (text.Contains("/") || text.Contains(@"\"))
    {
        text = File.ReadAllText(text);
    }

    Console.WriteLine($"Sending request to Amazon Polly to synthesize speech from text: {text}");

    var isSSML = text.StartsWith("<speak>");

    var request = new SynthesizeSpeechRequest
    {
        LanguageCode = languageCode,
        VoiceId = (VoiceId)voiceId,
        Text = text,
        OutputFormat = OutputFormat.Mp3,
        LexiconNames = (args.Length > 3) ? new List<string>(args[4].Split(',')) : null,
        TextType = isSSML ? TextType.Ssml : TextType.Text,
        Engine = isSSML ? Engine.Standard : Engine.Neural
    };

    var response = await pollyClient.SynthesizeSpeechAsync(request);

    if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
    {
        Console.WriteLine($"Synthesized successfully");

        var mp3Filename = (@"polly-tts.mp3");

        using (var fileStream = File.Create(mp3Filename))
        {
            response.AudioStream.CopyTo(fileStream);
            fileStream.Flush();
            fileStream.Close();
        }

        Console.WriteLine($"Audio stream saved to {mp3Filename}");
    }
    else
    {
        Console.WriteLine($"Error: SynthesizeSpeechAsync returned HTTP status code {response.HttpStatusCode}");
    }
}
else
{
    Console.WriteLine("Usage: dotnet run -- languageCode voiceCode text|file [lexicon-names]");
    Console.WriteLine("Ex:    dotnet run -- en-US Mike \"Hello there. My name is Mike.\"");
    Console.WriteLine("Ex:    dotnet run -- en-US Amy \"data\\article.txt\"");
}
