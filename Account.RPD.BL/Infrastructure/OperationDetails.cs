namespace AccountRPD.BL.Infrastructure
{
    public class OperationDetails
    {
        /// <summary>
        /// Успешно?
        /// </summary>
        public bool Succedeed { get; private set; }

        /// <summary>
        /// Сообщение о выполнении операции
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Свойство, создавшее операцию
        /// </summary>
        public string Property { get; private set; }

        public OperationDetails(bool succedeed, string message, string property)
        {
            Succedeed = succedeed;
            Message = message;
            Property = property;
        }
    }
}
