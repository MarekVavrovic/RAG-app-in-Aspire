using IcmChatAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using System.ComponentModel;
using ChatResponse = IcmChatAPI.Models.ChatResponse;

namespace IcmChatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatClient _chatClient;
        private readonly VectorStoreCollection<Guid, IcmChunk> _icmCollection;
        private readonly ChatOptions _chatOptions = new();

        public ChatController(IChatClient chatClient, VectorStoreCollection<Guid, IcmChunk> icmCollection)
        {
            _chatClient = chatClient;
            _icmCollection = icmCollection;
            _chatOptions.Tools = [
                AIFunctionFactory.Create(SearchIcmAsync)
            ];
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponse>> SendQuery([FromBody] ChatRequest query)
        {
            try
            {
                List<ChatMessage> messages =
                [
                    new(ChatRole.System, SystemPrompt),
                    new(ChatRole.User, query.Query)
                ];

                var response = await _chatClient.GetResponseAsync(messages, _chatOptions);

                return Ok(new ChatResponse { Message = response.Text, Status = "Success" });
            }
            catch (OperationCanceledException)
            {
                return StatusCode(StatusCodes.Status499ClientClosedRequest);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Description("Search for information using a phrase or keyword. Relies on ICMs which already being loaded.")]
        private async Task<IEnumerable<string>> SearchIcmAsync([Description("The phrase to search for.")] string searchPhrase)
        {
            var nearest = _icmCollection.SearchAsync(searchPhrase, top: 5);
            return await nearest.Select(result => result.Record.Content).ToListAsync();
        }

        private const string SystemPrompt = """
                                            You are an ICM Buddy assistant that answers questions ONLY using information retrieved from ICM incidents.

                                            Rules:
                                            - You MUST call the SearchIcmAsync tool before answering any question.
                                            - Use SearchIcmAsync with relevant keywords extracted from the user's question.
                                            - Answer ONLY using the information returned by SearchIcmAsync.
                                            - Do NOT use external knowledge.
                                            - Do NOT guess or speculate.
                                            - If the retrieved information is empty or not sufficient, respond with:
                                              "Not enough data in ICM history."

                                            Response format:
                                            - Use simple markdown only.
                                            - Structure your response as follows:

                                            Diagnosis:
                                            - Brief explanation based on retrieved incidents.

                                            Recommended Actions:
                                            - Concrete actions taken in past incidents.

                                            Related Incidents:
                                            - List incident IDs and a short reason for relevance.



                                                    
                                            """;
    }
}
