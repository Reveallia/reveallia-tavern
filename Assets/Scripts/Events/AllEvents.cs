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
        public Customer Customer;
        public CharacterDestination Destination;
        public ProgressState Progress;
        
        public DestinationStatusChanged(Customer customer, CharacterDestination destination, ProgressState progress)
        {
            Customer = customer;
            Destination = destination;
            Progress = progress;
        }
    }
    
    public class DestinationStatusChangedConverter : JsonConverter<DestinationStatusChanged>
    {
        public override void WriteJson(JsonWriter writer, DestinationStatusChanged value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("CharacterName");
            serializer.Serialize(writer, value.Customer.CharacterData.CharacterName);
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