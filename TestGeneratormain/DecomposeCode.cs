using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestGenerator.Context;

namespace TestGenerator
{

    public class DecomposeCode
    {
        string GetTypeName(TypeDeclarationSyntax type)
        {
            return type.Identifier.ValueText;
        }

        string GetNamespace(NamespaceDeclarationSyntax namesp)
        {
            return namesp.Name.ToString();
        }

        private MethodContext[] GetMethodsNames(TypeDeclarationSyntax type)
        {
            var publicMethods = from methodDeclaration in type.DescendantNodes()
                                                                                 .OfType<MethodDeclarationSyntax>()
                                where methodDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
                                select methodDeclaration;
            MethodContext[] methodContexts = new MethodContext[publicMethods.Count()];
            int i = 0;
            foreach (var method in publicMethods)
            {
                methodContexts[i] = new MethodContext();
                methodContexts[i].Name = method.Identifier.ValueText;
                methodContexts[i].ReturnVal = method.ReturnType.ToString();
                foreach (var param in method.ParameterList.Parameters)
                {
                    methodContexts[i].Parameters.Add((param.Type.ToString(), param.Identifier.ValueText));
                }
                i++;
            }
            return methodContexts;
        }

        private ConstructorContext getOptimal(ConstructorContext first, ConstructorContext second) {
            var firstParams = first.getCount();
            var secondParams = second.getCount();
            if (firstParams.depend > secondParams.depend)
            {
                return first;
            }
            else if (firstParams.depend == secondParams.depend) {
                if (firstParams.other <= secondParams.other) {
                    return first;
                }
            }
            return second;
        }

        private ConstructorContext SearchOptimalConstructor(IEnumerable<ConstructorDeclarationSyntax> constructorDeclarations) {
            var contextWithMaxDepMinOtherparamCount = new ConstructorContext();
            string dependencyPattren = @"I[^s]\w*";
            if (constructorDeclarations == null)
            {
                return null;
            }
            foreach (var constructor in constructorDeclarations)
            {
                var constructorContext = new ConstructorContext();
                foreach (var parm in constructor.ParameterList.Parameters)
                {
                    bool dependencuFl = Regex.IsMatch(parm.Type.ToString(), dependencyPattren);
                    constructorContext.Parameters.Add((parm.Type.ToString(), parm.Identifier.ValueText, dependencuFl));
                }
                contextWithMaxDepMinOtherparamCount = getOptimal(contextWithMaxDepMinOtherparamCount,constructorContext);
            }
            return contextWithMaxDepMinOtherparamCount;
        }

        private ConstructorContext GetConstructor(TypeDeclarationSyntax type)
        {
            var publicConstructors = from constructorDeclaration in type.DescendantNodes()
                                                                                 .OfType<ConstructorDeclarationSyntax>()
                                where constructorDeclaration.Modifiers.Any(SyntaxKind.PublicKeyword)
                                select constructorDeclaration;
            var constructor = SearchOptimalConstructor(publicConstructors);
            return constructor;
        }
        public IEnumerable<TypeContext> DecomposeType(string programText)
        {
            var list = new List<TypeContext>();
            SyntaxTree tree = CSharpSyntaxTree.ParseText(programText);
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            foreach (var member in root.Members)
            {
                var namespaceDecl = member as NamespaceDeclarationSyntax;
                if (namespaceDecl != null)
                {
                    foreach (var memberType in namespaceDecl.Members)
                    {
                        if (memberType.Kind() == SyntaxKind.ClassDeclaration || memberType.Kind() == SyntaxKind.StructDeclaration)
                        {
                            var type = memberType as TypeDeclarationSyntax;

                            list.Add(new TypeContext(GetNamespace(namespaceDecl), GetTypeName(type), GetMethodsNames(type),GetConstructor(type)));
                        }
                    }
                }
            }
            return list;
        }

        public async Task<IEnumerable<TypeContext>> AsyncDecomposeType(string programText)
        {
            return DecomposeType(programText);
        }
    }
}
