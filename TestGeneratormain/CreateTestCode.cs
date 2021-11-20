using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGenerator.Context;
using TestGeneratormain.Context;

namespace TestGenerator
{
    public class CreateTestCode
    {

        private CompilationUnitSyntax SetUsingDirectives(string classNamespace, CompilationUnitSyntax syntaxFactory) {
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")));
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Linq")));
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Text")));
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Moq")));
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.VisualStudio.TestTools.UnitTesting")));
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(classNamespace)));
            return syntaxFactory;
        }


        private ClassDeclarationSyntax CreateFields(TypeContext context, ClassDeclarationSyntax classDeclaration)
        {
            foreach (var param in context.Constructor.Parameters)
            {
                if (param.isDependency)
                {
                    FieldDeclarationSyntax field = SyntaxFactory.FieldDeclaration(
                                                    SyntaxFactory.VariableDeclaration(
                                                        SyntaxFactory.ParseTypeName($"Mock<{param.type}>"),
                                                        SyntaxFactory.SeparatedList(new[] { SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(param.name)) })
                                                    ))
                                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
                    classDeclaration = classDeclaration.AddMembers(field);
                }
            }
            FieldDeclarationSyntax classF = SyntaxFactory.FieldDeclaration(
                                            SyntaxFactory.VariableDeclaration(
                                                SyntaxFactory.ParseTypeName(context.Type),
                                                SyntaxFactory.SeparatedList(new[] { SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(context.Name)) })
                                            ))
                                            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));
            classDeclaration = classDeclaration.AddMembers(classF);
            return classDeclaration;
        }
        private MethodDeclarationSyntax CreateInitializationMethod(TypeContext context) {
            if (context.Constructor.Parameters == null) {
                return null;
            }
            
            List<StatementSyntax> syntax = new List<StatementSyntax>();
            var constructorString = new StringBuilder();
            constructorString.Append($"{context.Name} = new {context.Type}(");
            foreach (var param in context.Constructor.Parameters)
            {
                
                if (param.isDependency) {
                    constructorString.Append($"{param.name}.Object,");
                    syntax.Add(SyntaxFactory.ParseStatement($"{param.name} = new Mock<{param.type}>();"));
                }
                else
                {
                    constructorString.Append($"{param.name},");
                    syntax.Add(SyntaxFactory.ParseStatement($"{param.type} {param.name} = default;"));
                }
            }
            
            if (context.Constructor.Parameters.Count != 0) {
                constructorString.Remove(constructorString.Length - 1,1);
            }
            constructorString.Append(");");

            syntax.Add(SyntaxFactory.ParseStatement(constructorString.ToString()));

            var attribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("TestInitialize()")))
                    ).NormalizeWhitespace();

            var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Initialization")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block(syntax))
                    .AddAttributeLists(attribute);

            return methodDeclaration;
        }

        private IEnumerable<StatementSyntax> GenerateBody(TypeContext context,MethodContext method) {
            var body = new List<StatementSyntax>();
            int arrangeInd = 0;
            int actInd = 0;
            int assertInd = 0;

            //Arrange
            var args = new StringBuilder();
            if (method.hasArgs())
            {
                foreach (var param in method.Parameters)
                {
                    actInd++;
                    assertInd++;
                    args.Append(param.name + ",");
                    body.Add(SyntaxFactory.ParseStatement($"{param.type} {param.name} = default;"));
                }
                if (method.Parameters.Count != 0)
                {
                    args.Remove(args.Length - 1, 1);
                }
            }

            //Act
            if (method.hasReturn())
            {
                assertInd++;
                body.Add(SyntaxFactory.ParseStatement($"{method.ReturnVal} actual = {context.Name}.{method.Name}({args});"));
            }

            // Assert
            if (method.hasReturn())
            {
                body.Add(SyntaxFactory.ParseStatement($"{method.ReturnVal} expected  = default;"));
                body.Add(SyntaxFactory.ParseStatement("Assert.AreEqual(actual, expected);"));
            }
            body.Add(SyntaxFactory.ParseStatement(" Assert.Fail(\"autogenerated\");"));
            body[assertInd] = body[assertInd].WithLeadingTrivia(SyntaxFactory.Comment("\n\t    // Assert"));
            body[actInd] = body[actInd].WithLeadingTrivia(SyntaxFactory.Comment( "\n\t    // Act"+body[actInd].GetLeadingTrivia().ToFullString()));
            body[arrangeInd] = body[arrangeInd].WithLeadingTrivia(SyntaxFactory.Comment("\n\t    // Arrange" + body[arrangeInd].GetLeadingTrivia().ToFullString()));
            return body;
        }

        private MethodDeclarationSyntax[] CreateTestMethods(TypeContext context) {
            var testMethods = new List<MethodDeclarationSyntax>();
            var attribute = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("TestMethod()")))
                    ).NormalizeWhitespace();
            foreach (var method in context.Methods)
            {
                var methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), method.Name + "Test")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block(GenerateBody(context,method)))
                    .AddAttributeLists(attribute);
                testMethods.Add(methodDeclaration);
            }
            return testMethods.ToArray();
        }

        private ClassDeclarationSyntax Create_Class(TypeContext context)
        { 
            //  Create a class0
            var classDeclaration = SyntaxFactory.ClassDeclaration(context.Type + "Test");
            var classAttr = SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("TestClass()")).NormalizeWhitespace());
            // Add the public modifier
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                                               .AddAttributeLists(SyntaxFactory.AttributeList(classAttr));
            classDeclaration = CreateFields(context, classDeclaration);
            classDeclaration = classDeclaration.AddMembers(CreateInitializationMethod(context));
            classDeclaration = classDeclaration.AddMembers(CreateTestMethods(context));
            return classDeclaration;
        }

        private NamespaceDeclarationSyntax CreateNamespace(TypeContext context) {
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(context.Namespace + ".Test")).NormalizeWhitespace();
            
            // Add the class to the namespace.
            @namespace = @namespace.AddMembers(Create_Class(context));
            return @namespace;
        }


        /// <summary>
        /// Create a class from scratch.
        /// </summary>
        public Code CreateTestClass(TypeContext context)
        {
            // Create CompilationUnitSyntax
            var syntaxFactory = SyntaxFactory.CompilationUnit();

            // Add System using statement: (using  Microsoft.VisualStudio.TestTools.UnitTesting)
            syntaxFactory = SetUsingDirectives(context.Namespace, syntaxFactory);

            syntaxFactory = syntaxFactory.AddMembers(CreateNamespace(context));
            
            // Normalize and get code as string.
            Code code =new Code() {CodeText=syntaxFactory.NormalizeWhitespace().ToFullString(),
                                    Name=context.Namespace + "_Test_" + context.Name};
            return code;
        }

        public async Task<Code> AsyncCreateTestClass(TypeContext context)
        {
            return CreateTestClass(context);
        }
    }
}