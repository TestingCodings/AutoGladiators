"""
Simple test script to demonstrate the FileLogger system functionality.
This shows what the log files will contain and where they're saved.
"""
import os
import datetime

def create_example_logs():
    """Create example log files to show what the FileLogger will produce."""
    
    # Create the log directory path (same as in FileLogger.cs)
    documents_path = os.path.expanduser("~/Documents")
    log_dir = os.path.join(documents_path, "AutoGladiators", "Logs")
    
    # Create directory if it doesn't exist
    os.makedirs(log_dir, exist_ok=True)
    
    # Get current date for file names
    today = datetime.datetime.now().strftime("%Y%m%d")
    
    # Example battle log
    battle_log_path = os.path.join(log_dir, f"battle_{today}.log")
    battle_log_content = f"""[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Battle started between Thunder (Player) and Shadow Stalker (Enemy)
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Thunder used Lightning Strike for 45 damage
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Shadow Stalker used Shadow Punch for 38 damage
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Thunder used Health Potion, restored 50 HP
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Shadow Stalker used Energy Beam for 52 damage
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Thunder used Thunder Storm (Ultimate) for 95 damage
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Shadow Stalker was defeated!
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Thunder gained 150 experience points!
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Battle ended - Thunder is victorious!
"""
    
    # Example system log
    system_log_path = os.path.join(log_dir, f"system_{today}.log")
    system_log_content = f"""[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Application started - Enhanced Battle System
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] FileLogger initialized - Log directory: {log_dir}
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Enhanced Battle Page loaded with MP-based combat system
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Battle items loaded: 8 types available
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Enhanced moves loaded: 4-tier system (Basic→Advanced→Special→Ultimate)
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Player bot Thunder initialized - Level 5, 150 HP, 65 MP
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Enemy bot Shadow Stalker initialized - Level 4, 120 HP, 50 MP
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] Debug menu accessed - Log viewer opened
"""
    
    # Example error log
    error_log_path = os.path.join(log_dir, f"error_{today}.log")
    error_log_content = f"""[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] WARNING: Move validation - Ultimate move attempted without sufficient MP
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] INFO: Battle item cooldown active - Health Potion on 3-turn cooldown
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] DEBUG: MP regeneration triggered - Player gained 3 MP at turn end
[{datetime.datetime.now().strftime('%Y-%m-%d %H:%M:%S')}] INFO: Combo requirement met - Advanced move unlocked after basic move
"""
    
    # Write the example logs
    with open(battle_log_path, 'w', encoding='utf-8') as f:
        f.write(battle_log_content)
    
    with open(system_log_path, 'w', encoding='utf-8') as f:
        f.write(system_log_content)
    
    with open(error_log_path, 'w', encoding='utf-8') as f:
        f.write(error_log_content)
    
    print(f"Example log files created in: {log_dir}")
    print(f"- Battle log: {os.path.basename(battle_log_path)}")
    print(f"- System log: {os.path.basename(system_log_path)}")
    print(f"- Error log: {os.path.basename(error_log_path)}")
    print(f"\nThese logs demonstrate what the FileLogger system will create when you play battles.")
    print(f"Access them via: Debug Menu → '📄 Access Debug Logs' → Log Viewer")

if __name__ == "__main__":
    create_example_logs()