using System;
using Characters;
using Managers;
using Newtonsoft.Json;

namespace Events
{
    [Serializable]
    public struct DayCycleChanged
    {
        public TimeOfDay NewState;
    }
    
    [Serializable]
    [JsonConverter(typeof(DestinationStatusChangedConverter))]
    public struct DestinationStatusChanged
    {
        public Character Character;
        public CharacterDestination Destination;
        public ProgressState Progress;
        
        public DestinationStatusChanged(Customer character, CharacterDestination destination, ProgressState progress)
        {
            Character = character;
            Destination = destination;
            Progress = progress;
        }

        public bool IsCustomer()
        {
            return Character is Customer;
        }

        public bool IsReceptionCompleted()
        {
            return Destination == CharacterDestination.Reception && Progress == ProgressState.Completed;
        }
    }
    
    [Serializable]
    public struct OrderPublished
    {
        public string OrderId;
    
        public OrderPublished(Order order)
        {
            OrderId = order.OrderId;
        }
    }
    
    public struct OrderAccepted
    {
        public string OrderId;

        public OrderAccepted(Order order)
        {
            OrderId = order.OrderId;
        }
    }
    
    public class DestinationStatusChangedConverter : JsonConverter<DestinationStatusChanged>
    {
        public override void WriteJson(JsonWriter writer, DestinationStatusChanged value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("CharacterName");
            serializer.Serialize(writer, value.Character.CharacterData.CharacterName);
            writer.WritePropertyName("Destination");
            serializer.Serialize(writer, value.Destination);
            writer.WritePropertyName("Progress");
            serializer.Serialize(writer, value.Progress);
            writer.WriteEndObject();
        }

        public override DestinationStatusChanged ReadJson(JsonReader reader, Type objectType, DestinationStatusChanged existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    
    
    public enum ProgressState
    {
        None,
        InProgress,
        Completed,
        Failed
    }
}