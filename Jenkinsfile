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

        stage('Build Android Client') {
            steps {
                bat 'dotnet build AutoGladiators.Client\\AutoGladiators_MAUI.csproj --configuration Release -f net8.0-android'
            }
        }

        stage('Publish APK') {
            steps {
                bat 'dotnet publish AutoGladiators.Client\\AutoGladiators_MAUI.csproj --configuration Release -f net8.0-android -o publish\\'
            }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: 'publish/**/*', allowEmptyArchive: true
        }
    }
}
