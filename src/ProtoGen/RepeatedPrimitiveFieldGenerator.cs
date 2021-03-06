// Protocol Buffers - Google's data interchange format
// Copyright 2008 Google Inc.  All rights reserved.
// http://github.com/jskeet/dotnet-protobufs/
// Original C++/Java/Python code:
// http://code.google.com/p/protobuf/
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Collections.Generic;
using System.Text;
using Google.ProtocolBuffers.DescriptorProtos;
using Google.ProtocolBuffers.Descriptors;

namespace Google.ProtocolBuffers.ProtoGen {
  internal class RepeatedPrimitiveFieldGenerator : FieldGeneratorBase, IFieldSourceGenerator {

    internal RepeatedPrimitiveFieldGenerator(FieldDescriptor descriptor)
      : base(descriptor) {
    }

    public void GenerateMembers(TextGenerator writer) {
      if (Descriptor.IsPacked && Descriptor.File.Options.OptimizeFor == FileOptions.Types.OptimizeMode.SPEED) {
        writer.WriteLine("private int {0}MemoizedSerializedSize;", Name);
      }
      writer.WriteLine("private pbc::PopsicleList<{0}> {1}_ = new pbc::PopsicleList<{0}>();", TypeName, Name);
      AddClsComplianceCheck(writer);
      writer.WriteLine("public scg::IList<{0}> {1}List {{", TypeName, PropertyName);
      writer.WriteLine("  get {{ return pbc::Lists.AsReadOnly({0}_); }}", Name);
      writer.WriteLine("}");

      // TODO(jonskeet): Redundant API calls? Possibly - include for portability though. Maybe create an option.
      writer.WriteLine("public int {0}Count {{", PropertyName);
      writer.WriteLine("  get {{ return {0}_.Count; }}", Name);
      writer.WriteLine("}");

      AddClsComplianceCheck(writer);
      writer.WriteLine("public {0} Get{1}(int index) {{", TypeName, PropertyName);
      writer.WriteLine("  return {0}_[index];", Name);
      writer.WriteLine("}");
    }

    public void GenerateBuilderMembers(TextGenerator writer) {
      // Note:  We can return the original list here, because we make it unmodifiable when we build
      // We return it via IPopsicleList so that collection initializers work more pleasantly.
      AddClsComplianceCheck(writer);
      writer.WriteLine("public pbc::IPopsicleList<{0}> {1}List {{", TypeName, PropertyName);
      writer.WriteLine("  get {{ return result.{0}_; }}", Name);
      writer.WriteLine("}");
      writer.WriteLine("public int {0}Count {{", PropertyName);
      writer.WriteLine("  get {{ return result.{0}Count; }}", PropertyName);
      writer.WriteLine("}");
      AddClsComplianceCheck(writer);
      writer.WriteLine("public {0} Get{1}(int index) {{", TypeName, PropertyName);
      writer.WriteLine("  return result.Get{0}(index);", PropertyName);
      writer.WriteLine("}");
      AddClsComplianceCheck(writer);
      writer.WriteLine("public Builder Set{0}(int index, {1} value) {{", PropertyName, TypeName);
      AddNullCheck(writer);
      writer.WriteLine("  result.{0}_[index] = value;", Name);
      writer.WriteLine("  return this;");
      writer.WriteLine("}");
      AddClsComplianceCheck(writer);
      writer.WriteLine("public Builder Add{0}({1} value) {{", PropertyName, TypeName);
      AddNullCheck(writer);
      writer.WriteLine("  result.{0}_.Add(value);", Name, TypeName);
      writer.WriteLine("  return this;");
      writer.WriteLine("}");
      AddClsComplianceCheck(writer);
      writer.WriteLine("public Builder AddRange{0}(scg::IEnumerable<{1}> values) {{", PropertyName, TypeName);
      writer.WriteLine("  base.AddRange(values, result.{0}_);", Name);
      writer.WriteLine("  return this;");
      writer.WriteLine("}");
      writer.WriteLine("public Builder Clear{0}() {{", PropertyName);
      writer.WriteLine("  result.{0}_.Clear();", Name);
      writer.WriteLine("  return this;");
      writer.WriteLine("}");
    }

    public void GenerateMergingCode(TextGenerator writer) {
      writer.WriteLine("if (other.{0}_.Count != 0) {{", Name);
      writer.WriteLine("  base.AddRange(other.{0}_, result.{0}_);", Name);
      writer.WriteLine("}");
    }

    public void GenerateBuildingCode(TextGenerator writer) {
      writer.WriteLine("result.{0}_.MakeReadOnly();", Name);
    }

    public void GenerateParsingCode(TextGenerator writer) {
      if (Descriptor.IsPacked) {
        writer.WriteLine("int length = input.ReadInt32();");
        writer.WriteLine("int limit = input.PushLimit(length);");
        writer.WriteLine("while (!input.ReachedLimit) {");
        writer.WriteLine("  Add{0}(input.Read{1}());", PropertyName, CapitalizedTypeName);
        writer.WriteLine("}");
        writer.WriteLine("input.PopLimit(limit);");
      } else {
        writer.WriteLine("Add{0}(input.Read{1}());", PropertyName, CapitalizedTypeName);
      }
    }

    public void GenerateSerializationCode(TextGenerator writer) {
      writer.WriteLine("if ({0}_.Count > 0) {{", Name);
      writer.Indent();
      if (Descriptor.IsPacked) {
        writer.WriteLine("output.WriteRawVarint32({0});", WireFormat.MakeTag(Descriptor));
        writer.WriteLine("output.WriteRawVarint32((uint) {0}MemoizedSerializedSize);", Name);
        writer.WriteLine("foreach ({0} element in {1}_) {{", TypeName, Name);
        writer.WriteLine("  output.Write{0}NoTag(element);", CapitalizedTypeName);
        writer.WriteLine("}");
      } else {
        writer.WriteLine("foreach ({0} element in {1}_) {{", TypeName, Name);
        writer.WriteLine("  output.Write{0}({1}, element);", CapitalizedTypeName, Number);
        writer.WriteLine("}");
      }
      writer.Outdent();
      writer.WriteLine("}");
    }

    public void GenerateSerializedSizeCode(TextGenerator writer) {
      writer.WriteLine("{");
      writer.Indent();
      writer.WriteLine("int dataSize = 0;");
      if (FixedSize == -1) {
        writer.WriteLine("foreach ({0} element in {1}List) {{", TypeName, PropertyName);
        writer.WriteLine("  dataSize += pb::CodedOutputStream.Compute{0}SizeNoTag(element);", CapitalizedTypeName, Number);
        writer.WriteLine("}");
      } else {
        writer.WriteLine("dataSize = {0} * {1}_.Count;", FixedSize, Name);
      }
      writer.WriteLine("size += dataSize;");
      int tagSize = CodedOutputStream.ComputeTagSize(Descriptor.FieldNumber);
      if (Descriptor.IsPacked) {
        writer.WriteLine("size += {0};", tagSize);
        writer.WriteLine("size += pb::CodedOutputStream.ComputeInt32SizeNoTag(dataSize);");
      } else {
        writer.WriteLine("size += {0} * {1}_.Count;", tagSize, Name);
      }
      // cache the data size for packed fields.
      if (Descriptor.IsPacked) {
        writer.WriteLine("{0}MemoizedSerializedSize = dataSize;", Name);
      }
      writer.Outdent();
      writer.WriteLine("}");
    }
  }
}
