using Gate.LangBase.Expressions;
using Gate.LangBase.Runtime.DbgEng;

namespace Gate.LangBase.Runtime.Object
{
   /// <summary>
   /// Defines a strategy for handling real-time messaging (RTM) objects, including operations for  parameter copying,
   /// type assignments, and object manipulation.
   /// </summary>
   /// <remarks>The <see cref="IRtmObjStrategy"/> interface provides a set of methods and properties to
   /// facilitate  the management and transformation of RTM objects in a real-time messaging system. It includes 
   /// functionality for creating and modifying RTM parameters, handling type assignments, and accessing  or
   /// manipulating object members. Implementations of this interface are expected to define the  specific behavior for
   /// these operations.</remarks>
   public interface IRtmObjStrategy
   {
      /// <summary>
      /// Gets the operator modifier used to customize the behavior of the real-time messaging operator.
      /// </summary>
      /// <remarks>The <see cref="OperatorModifier"/> property provides a mechanism to modify or extend the
      /// behavior of the real-time messaging operator. Implementations of <see cref="IRtmOperatorModifier"/> can be
      /// used to apply specific rules or transformations.</remarks>
      IRtmOperatorModifier OperatorModifier { get; }

      /// <summary>
      /// Gets the handler modifier for C# operations in the RTM (Real-Time Messaging) system.
      /// </summary>
      IRtmOperatorCSharpHandlerModifier CSharpHandlerModifier { get; }

      /// <summary>
      /// Create a parameter rtm-value for function call by reference ( <see cref="RtmObj.Address"/> is copied.).
      /// </summary>
      /// <param name="paramValue"> <see cref="RtmObj"/> parameter value.</param>
      /// <param name="paramDecl">Declaration instance of parameter.</param>
      /// <returns></returns>
      RtmObj CopyFunctionParamByRef(RtmObj paramValue, IDecl paramDecl);

      /// <summary>
      /// Create a parameter rtm-value for function call by value 
      /// ( returned <see cref="RtmObj.Address"/> is allocated, then content is copied).
      /// </summary>
      /// <param name="paramValue"> <see cref="RtmObj"/> parameter value.</param>
      /// <param name="paramDecl">Declaration instance of parameter.</param>
      /// <returns></returns>
      RtmObj CopyFunctionParamByValue(RtmObj paramValue, IDecl paramDecl);

      /// <summary>
      /// <br> Create an anonimous parameter rtm-value for function call by value</br>
      /// <br>( returned <see cref="RtmObj.Address"/> is allocated with DeclType = param.DeclType , then content is copied).</br>
      /// </summary>
      /// <param name="paramValue"> <see cref="RtmObj"/> parameter value.</param>
      /// <returns></returns>
      RtmObj CopyFunctionOptionalParamByValue(RtmObj paramValue);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmObj"></param>
      /// <param name="indices"></param>
      /// <returns></returns>
      RtmObj GetArrayItem(RtmObj rtmObj, params int[] indices);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="declType"></param>
      /// <returns></returns>
      Type GetCSharpType(IDeclType? declType);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmObj"></param>
      /// <returns></returns>
      RtmObj[] GetRecordMembers(RtmObj rtmObj);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmObj"></param>
      /// <param name="memberId"></param>
      /// <returns></returns>
      RtmObj GetRecordMember(RtmObj rtmObj, string memberId);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="constValue"></param>
      /// <param name="declType"></param>
      /// <returns></returns>
      RtmObj MakeConstant(ValueType constValue, IDeclType? declType);

      /// <summary>
      /// Creates a new runtime object based on the specified declaration and allocator context.
      /// </summary>
      /// <param name="exprDecl">The declaration that defines the structure and behavior of the runtime object to create. Cannot be null.</param>
      /// <returns>A new instance of <see cref="RtmObj"/> initialized according to the provided declaration and allocator
      /// context.</returns>
      RtmObj MakeNewObject(IDecl exprDecl);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="paramValue"></param>
      /// <param name="declType"></param>
      /// <returns></returns>
      RtmObj MakeRefValue(RtmObj paramValue, IDeclType declType);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="lObject"></param>
      /// <param name="rObject"></param>
      /// <returns></returns>
      RtmObj Assign(RtmObj lObject, RtmObj rObject);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="lType"></param>
      /// <param name="rType"></param>
      /// <param name="assignContext"></param>
      /// <returns></returns>
      bool CanAssignTypeTo(IDeclType lType, IDeclType rType, RtmObjStrategyAssignContext assignContext);

      /// <summary>
      /// 
      /// </summary>
      /// <param name="rtmObj"></param>
      /// <returns></returns>
      IRtmObjFunction? GetFunction(RtmObj? rtmObj);

      /// <summary>
      /// Get all function visible objects from stack.
      /// </summary>
      /// <param name="stack"></param>
      /// <returns></returns>
      RtmObj[]? GetFunctionVisibleObject(IRtmDbgEngStackExecutable? stack);

      /// <summary>
      /// Get function parameters by declaration and arguments.
      /// </summary>
      /// <param name="declParams"></param>
      /// <param name="rtmArgs"></param>
      /// <returns></returns>
      RtmObj[] GetParams(IDecl[] declParams, RtmObj[] rtmArgs);
   }
}

