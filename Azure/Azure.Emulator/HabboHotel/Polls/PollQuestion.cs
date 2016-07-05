using Azure.Messages;
using System.Collections.Generic;
using System.Linq;

namespace Azure.HabboHotel.Polls
{
    /// <summary>
    /// Class PollQuestion.
    /// </summary>
    internal class PollQuestion
    {
        /// <summary>
        /// The index
        /// </summary>
        internal uint Index;

        /// <summary>
        /// The question
        /// </summary>
        internal string Question;

        /// <summary>
        /// a type
        /// </summary>
        internal PollAnswerType AType;

        /// <summary>
        /// The answers
        /// </summary>
        internal List<string> Answers = new List<string>();

        /// <summary>
        /// The correct answer
        /// </summary>
        internal string CorrectAnswer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PollQuestion"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="question">The question.</param>
        /// <param name="aType">a type.</param>
        /// <param name="answers">The answers.</param>
        /// <param name="correctAnswer">The correct answer.</param>
        internal PollQuestion(uint index, string question, int aType, IEnumerable<string> answers, string correctAnswer)
        {
            this.Index = index;
            this.Question = question;
            this.AType = (PollAnswerType)aType;
            this.Answers = answers.ToList();
            this.CorrectAnswer = correctAnswer;
        }

        /// <summary>
        /// Enum PollAnswerType
        /// </summary>
        internal enum PollAnswerType
        {
            /// <summary>
            /// The radio selection
            /// </summary>
            RadioSelection = 1,

            /// <summary>
            /// The selection
            /// </summary>
            Selection = 2,

            /// <summary>
            /// The text
            /// </summary>
            Text = 3,
        }

        /// <summary>
        /// Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="questionNumber">The question number.</param>
        public void Serialize(ServerMessage message, int questionNumber)
        {
            message.AppendInteger(this.Index);
            message.AppendInteger(questionNumber);
            message.AppendInteger((int)this.AType);
            message.AppendString(this.Question);
            if (this.AType != PollAnswerType.Selection && this.AType != PollAnswerType.RadioSelection)
            {
                return;
            }
            message.AppendInteger(1);
            message.AppendInteger(this.Answers.Count);
            foreach (string current in this.Answers)
            {
                message.AppendString(current);
                message.AppendString(current);
            }
        }
    }
}