namespace Gate.CLanguage.Types
{
   public interface IClassFieldGroupPolicy
   {
      ClassFieldGroupPolicyId Id { get; }

      string Name { get; }

      (CTypeClassFieldGroup[], int) GetFieldGroupsAndSizeof(ITypeClass typeClass);
   }
}