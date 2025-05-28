pipeline {
    agent any

    environment {
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 'true'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout([$class: 'GitSCM',
                    userRemoteConfigs: [[
                        url: 'https://github.com/TestingCodings/AutoGladiators.git',
                        credentialsId: 'github-token'
                    ]],
                    branches: [[name: '*/main']]
                ])
            }
        }

        stage('Build SharedLibs') {
            steps {
                bat 'dotnet build SharedLibs\\StateMachineLib\\StateMachineLib.csproj --configuration Release'
                bat 'dotnet build SharedLibs\\BehaviorEngineLib\\BehaviorEngineLib.csproj --configuration Release'
                bat 'dotnet build SharedLibs\\SkillTreeLib\\SkillTreeLib.csproj --configuration Release'
            }
        }

        stage('Build Game Client (Android)') {
            steps {
                bat 'dotnet build AutoGladiators.Client\\AutoGladiators_MAUI.csproj --configuration Release -f net8.0-android'
            }
        }

        stage('Publish APK') {
            steps {
                bat 'dotnet publish AutoGladiators.Client\\AutoGladiators_MAUI.csproj --configuration Release -f net8.0-android -o publish\\'
            }
        }

        // Optional: Add tests if you have any
        // stage('Run Tests') {
        //     steps {
        //         bat 'dotnet test Tests\\AutoGladiators.Tests.csproj --configuration Release'
        //     }
        // }

        stage('Archive APK') {
            steps {
                archiveArtifacts artifacts: 'publish\\**\\*.apk', allowEmptyArchive: true
            }
        }
    }

    post {
        failure {
            echo '❌ Build failed. Check console output above.'
        }
        success {
            echo '✅ Build and publish successful.'
        }
    }
}
// This Jenkinsfile is designed to build and publish the AutoGladiators game client for Android using .NET MAUI.