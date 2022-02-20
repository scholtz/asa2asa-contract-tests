/* 
 * for KMD HTTP API
 *
 * API for KMD (Key Management Daemon)
 *
 * OpenAPI spec version: 0.0.1
 * Contact: contact@algorand.com
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using SwaggerDateConverter = Algorand.Client.SwaggerDateConverter;

namespace Algorand.Kmd.Model
{
    /// <summary>
    /// APIV1POSTMultisigTransactionSignResponse is the repsonse to &#x60;POST /v1/multisig/sign&#x60; friendly:SignMultisigResponse
    /// </summary>
    [DataContract]
        public partial class APIV1POSTMultisigTransactionSignResponse :  IEquatable<APIV1POSTMultisigTransactionSignResponse>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="APIV1POSTMultisigTransactionSignResponse" /> class.
        /// </summary>
        /// <param name="error">error.</param>
        /// <param name="message">message.</param>
        /// <param name="multisig">multisig.</param>
        public APIV1POSTMultisigTransactionSignResponse(bool? error = default(bool?), string message = default(string), MultisigSig multisig = default(MultisigSig))
        {
            this.Error = error;
            this.Message = message;
            this.Multisig = multisig;
        }
        
        /// <summary>
        /// Gets or Sets Error
        /// </summary>
        [DataMember(Name="error", EmitDefaultValue=false)]
        public bool? Error { get; set; }

        /// <summary>
        /// Gets or Sets Message
        /// </summary>
        [DataMember(Name="message", EmitDefaultValue=false)]
        public string Message { get; set; }

        /// <summary>
        /// Gets or Sets Multisig
        /// </summary>
        [DataMember(Name="multisig", EmitDefaultValue=false)]
        public MultisigSig Multisig { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class APIV1POSTMultisigTransactionSignResponse {\n");
            sb.Append("  Error: ").Append(Error).Append("\n");
            sb.Append("  Message: ").Append(Message).Append("\n");
            sb.Append("  Multisig: ").Append(Multisig).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as APIV1POSTMultisigTransactionSignResponse);
        }

        /// <summary>
        /// Returns true if APIV1POSTMultisigTransactionSignResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of APIV1POSTMultisigTransactionSignResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(APIV1POSTMultisigTransactionSignResponse input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Error == input.Error ||
                    (this.Error != null &&
                    this.Error.Equals(input.Error))
                ) && 
                (
                    this.Message == input.Message ||
                    (this.Message != null &&
                    this.Message.Equals(input.Message))
                ) && 
                (
                    this.Multisig == input.Multisig ||
                    (this.Multisig != null &&
                    this.Multisig.Equals(input.Multisig))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Error != null)
                    hashCode = hashCode * 59 + this.Error.GetHashCode();
                if (this.Message != null)
                    hashCode = hashCode * 59 + this.Message.GetHashCode();
                if (this.Multisig != null)
                    hashCode = hashCode * 59 + this.Multisig.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }
}
