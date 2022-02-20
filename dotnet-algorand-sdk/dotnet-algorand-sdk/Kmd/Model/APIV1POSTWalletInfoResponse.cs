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
    /// APIV1POSTWalletInfoResponse is the response to &#x60;POST /v1/wallet/info&#x60; friendly:WalletInfoResponse
    /// </summary>
    [DataContract]
        public partial class APIV1POSTWalletInfoResponse :  IEquatable<APIV1POSTWalletInfoResponse>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="APIV1POSTWalletInfoResponse" /> class.
        /// </summary>
        /// <param name="error">error.</param>
        /// <param name="message">message.</param>
        /// <param name="walletHandle">walletHandle.</param>
        public APIV1POSTWalletInfoResponse(bool? error = default(bool?), string message = default(string), APIV1WalletHandle walletHandle = default(APIV1WalletHandle))
        {
            this.Error = error;
            this.Message = message;
            this.WalletHandle = walletHandle;
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
        /// Gets or Sets WalletHandle
        /// </summary>
        [DataMember(Name="wallet_handle", EmitDefaultValue=false)]
        public APIV1WalletHandle WalletHandle { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class APIV1POSTWalletInfoResponse {\n");
            sb.Append("  Error: ").Append(Error).Append("\n");
            sb.Append("  Message: ").Append(Message).Append("\n");
            sb.Append("  WalletHandle: ").Append(WalletHandle).Append("\n");
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
            return this.Equals(input as APIV1POSTWalletInfoResponse);
        }

        /// <summary>
        /// Returns true if APIV1POSTWalletInfoResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of APIV1POSTWalletInfoResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(APIV1POSTWalletInfoResponse input)
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
                    this.WalletHandle == input.WalletHandle ||
                    (this.WalletHandle != null &&
                    this.WalletHandle.Equals(input.WalletHandle))
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
                if (this.WalletHandle != null)
                    hashCode = hashCode * 59 + this.WalletHandle.GetHashCode();
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
