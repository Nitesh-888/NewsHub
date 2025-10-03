using System.ComponentModel.DataAnnotations;
using NewsHub.Models;

namespace NewsHub.DTOs.Reactions
{
    public class CreateReactionRequestDTO
    {
        [EnumDataType(typeof(ReactionType))]
        public ReactionType Type { get; set; }
    }
}
